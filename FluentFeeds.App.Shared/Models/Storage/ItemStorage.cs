using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentFeeds.App.Shared.EventArgs;
using FluentFeeds.App.Shared.Models.Database;
using FluentFeeds.App.Shared.Models.Items;
using FluentFeeds.App.Shared.Services;
using FluentFeeds.Feeds.Base;
using FluentFeeds.Feeds.Base.Items;
using FluentFeeds.Feeds.Base.Items.Content;
using Microsoft.EntityFrameworkCore;

namespace FluentFeeds.App.Shared.Models.Storage;

/// <summary>
/// Default item storage implementation.
/// </summary>
public sealed class ItemStorage : IItemStorage
{
	/// <summary>
	/// Content loader which lazily loads the serialized content from the database and then acts as a proxy for the
	/// deserialized loader.
	/// </summary>
	private sealed class ItemContentLoader : IItemContentLoader
	{
		public ItemContentLoader(
			IDatabaseService databaseService, ItemContentCache cache, FeedProvider provider, Guid identifier)
		{
			_databaseService = databaseService;
			_cache = cache;
			_provider = provider;
			_identifier = identifier;
		}

		public async Task<ItemContent> LoadAsync(bool reload = false, CancellationToken cancellation = default)
		{
			var loader = await _cache.GetLoaderAsync(_identifier, LoadFromDatabaseAsync);
			return await loader.LoadAsync(reload);
		}

		private Task<IItemContentLoader> LoadFromDatabaseAsync()
		{
			return _databaseService.ExecuteAsync(
				async database =>
				{
					var serialized = await database.Items
						.Where(i => i.Identifier == _identifier)
						.Select(i => i.Content)
						.FirstAsync();
					return await _provider.LoadItemContentAsync(serialized);
				});
		}

		private readonly IDatabaseService _databaseService;
		private readonly ItemContentCache _cache;
		private readonly FeedProvider _provider;
		private readonly Guid _identifier;
	}
	
	public ItemStorage(
		IDatabaseService databaseService, ItemContentCache itemContentCache, FeedProvider provider, Guid identifier)
	{
		Identifier = identifier;
		_databaseService = databaseService;
		_itemContentCache = itemContentCache;
		_provider = provider;

		_initialize = new Lazy<Task>(InitializeAsync);
	}
	
	public Guid Identifier { get; }
	
	public event EventHandler<ItemsDeletedEventArgs>? ItemsDeleted;
	
	public async Task<IEnumerable<IItemView>> GetItemsAsync(Guid feedIdentifier)
	{
		await _initialize.Value;
		
		return _feedItems.TryGetValue(feedIdentifier, out var items) ? items : Enumerable.Empty<IItemView>();
	}

	public async Task<IEnumerable<IItemView>> AddItemsAsync(IEnumerable<ItemDescriptor> items, Guid feedIdentifier)
	{
		await _initialize.Value;

		if (!_feedItems.TryGetValue(feedIdentifier, out var feedItems))
		{
			feedItems = new HashSet<Item>();
			_feedItems.Add(feedIdentifier, feedItems);
		}

		// Determine added and updated items, also initializing the result list.
		var added = new List<Item>();
		var addedToFeed = new List<Item>();
		var updated = new List<(Item, ItemDescriptor)>();
		var result = new List<IItemView>();
		foreach (var item in items)
		{
			// Check if there is an item in this storage with this identifier.
			if (item.Identifier != null && _identifierItems.TryGetValue(item.Identifier, out var existing))
			{
				// Check if the item needs to be updated.
				if (existing.ModifiedTimestamp < item.ModifiedTimestamp)
				{
					updated.Add((existing, item));
				}
				result.Add(existing);
				
				if (!feedItems.Contains(existing))
				{
					addedToFeed.Add(existing);
				}
			}
			else if (item.Identifier == null || !_deletedIdentifiers.Contains(item.Identifier))
			{
				// This is a new item.
				var storedItem = new Item(
					identifier: Guid.NewGuid(),
					storage: this,
					userIdentifier: item.Identifier,
					title: item.Title,
					author: item.Author,
					summary: item.Summary,
					publishedTimestamp: item.PublishedTimestamp,
					modifiedTimestamp: item.ModifiedTimestamp,
					url: item.Url,
					contentUrl: item.ContentUrl,
					isRead: false,
					contentLoader: item.ContentLoader);
				added.Add(storedItem);
				result.Add(storedItem);
			}
		}

		// Update database
		await _databaseService.ExecuteAsync(
			async database =>
			{
				foreach (var (item, updatedItem) in updated)
				{
					await UpdateItemAsync(database, item.Identifier, updatedItem);
				}
				await StoreItemsAsync(database, added);
				await MapItemsToFeedAsync(database, added, feedIdentifier);
				await MapItemsToFeedAsync(database, addedToFeed, feedIdentifier);
				await database.SaveChangesAsync();
			});
		
		// Update local copy
		foreach (var (item, updatedItem) in updated)
		{
			item.Url = updatedItem.Url;
			item.ContentUrl = updatedItem.ContentUrl;
			item.ModifiedTimestamp = updatedItem.ModifiedTimestamp;
			item.Title = updatedItem.Title;
			item.Author = updatedItem.Author;
			item.Summary = updatedItem.Summary;
			item.ContentLoader = updatedItem.ContentLoader;
		}
		
		foreach (var item in added)
		{
			RegisterItem(item);
			feedItems.Add(item);
		}

		foreach (var item in addedToFeed)
		{
			feedItems.Add(item);
		}

		return result;
	}

	public async Task<IItemView> SetItemReadAsync(Guid identifier, bool isRead)
	{
		var item = _items[identifier];
			
		// Update database
		await _databaseService.ExecuteAsync(
			async database =>
			{
				var dbItem = await database.Items.Where(i => i.Identifier == identifier).FirstAsync();
				dbItem.IsRead = isRead;
				await database.SaveChangesAsync();
			});
			
		// Update local copy
		item.IsRead = isRead;
		return item;
	}

	public async Task DeleteItemsAsync(IReadOnlyCollection<Guid> identifiers)
	{
		var items = new List<Item> { Capacity = identifiers.Count };
		var userIdentifiers = new List<string>();
		foreach (var identifier in identifiers)
		{
			if (_items.TryGetValue(identifier, out var item))
			{
				items.Add(item);
				if (item.UserIdentifier != null)
				{
					userIdentifiers.Add(item.UserIdentifier);
				}
			}
		}
		
		// Update database
		await _databaseService.ExecuteAsync(
			async database =>
			{
				foreach (var identifier in identifiers)
				{
					var dbItem = await database.Items.Where(i => i.Identifier == identifier).FirstAsync();
					database.Items.Remove(dbItem);

					var dbRelationships =
						await database.FeedItems.Where(i => i.ItemIdentifier == identifier).ToListAsync();
					database.FeedItems.RemoveRange(dbRelationships);
				}

				var dbDeletedItems = userIdentifiers
					.Select(identifier =>
						new DeletedItemDb
						{
							Identifier = Guid.NewGuid(),
							ProviderIdentifier = _provider.Metadata.Identifier,
							StorageIdentifier = Identifier,
							UserIdentifier = identifier
						});
				await database.DeletedItems.AddRangeAsync(dbDeletedItems);
				
				await database.SaveChangesAsync();
			});
			
		// Update local copy
		foreach (var identifier in identifiers)
		{
			_items.Remove(identifier);
		}

		foreach (var feedItems in _feedItems.Values)
		{
			foreach (var item in items)
			{
				feedItems.Remove(item);
			}
		}

		foreach (var userIdentifier in userIdentifiers)
		{
			_identifierItems.Remove(userIdentifier);
			_deletedIdentifiers.Add(userIdentifier);
		}
			
		ItemsDeleted?.Invoke(this, new ItemsDeletedEventArgs(items));
	}
	
	/// <summary>
	/// Add an item to the cache.
	/// </summary>
	private void RegisterItem(Item item)
	{
		_items.Add(item.Identifier, item);
		if (item.UserIdentifier != null)
		{
			_identifierItems.Add(item.UserIdentifier, item);
		}
	}
	
	/// <summary>
	/// Store a collection of items in the database.
	/// </summary>
	private async Task StoreItemsAsync(AppDbContext database, IReadOnlyCollection<Item> items)
	{
		var dbItems = new List<ItemDb> { Capacity = items.Count };
		foreach (var item in items)
		{
			dbItems.Add(
				new ItemDb
				{ 
					Identifier = item.Identifier, 
					ProviderIdentifier = _provider.Metadata.Identifier,
					StorageIdentifier = Identifier,
					UserIdentifier = item.UserIdentifier,
					Title = item.Title,
					Author = item.Author,
					Summary = item.Summary,
					Content = await _provider.StoreItemContentAsync(item.ContentLoader),
					PublishedTimestamp = item.PublishedTimestamp,
					ModifiedTimestamp = item.ModifiedTimestamp,
					Url = item.Url,
					ContentUrl = item.ContentUrl,
					IsRead = item.IsRead
				});
		}
		
		await database.Items.AddRangeAsync(dbItems);
	}

	private static async Task MapItemsToFeedAsync(AppDbContext database, IEnumerable<Item> items, Guid feedIdentifier)
	{
		var dbItems = items
			.Select(item =>
				new FeedItemDb
				{
					Identifier = Guid.NewGuid(),
					FeedIdentifier = feedIdentifier,
					ItemIdentifier = item.Identifier
				});
		await database.FeedItems.AddRangeAsync(dbItems);
	}

	/// <summary>
	/// Update an item in the database.
	/// </summary>
	private async Task UpdateItemAsync(AppDbContext database, Guid identifier, ItemDescriptor descriptor)
	{
		var dbItem = await database.Items.Where(i => i.Identifier == identifier).FirstAsync();
		dbItem.Url = descriptor.Url;
		dbItem.ContentUrl = descriptor.ContentUrl;
		dbItem.ModifiedTimestamp = descriptor.ModifiedTimestamp;
		dbItem.Title = descriptor.Title;
		dbItem.Author = descriptor.Author;
		dbItem.Summary = descriptor.Summary;
		dbItem.Content = await _provider.StoreItemContentAsync(descriptor.ContentLoader);
	}

	/// <summary>
	/// Load all items in this storage from the database into stored item models.
	/// </summary>
	private async Task LoadItemsAsync(AppDbContext database)
	{
		var providerIdentifier = _provider.Metadata.Identifier;
		var dbItems = await database.Items
			.Where(i => i.ProviderIdentifier == providerIdentifier && i.StorageIdentifier == Identifier)
			.Select(i =>
				new
				{
					i.Identifier,
					i.UserIdentifier,
					i.Title,
					i.Author,
					i.Summary,
					i.PublishedTimestamp,
					i.ModifiedTimestamp,
					i.Url,
					i.ContentUrl,
					i.IsRead
				})
			.ToListAsync();
		var items = dbItems
			.Select(item => new Item(
				identifier: item.Identifier,
				storage: this,
				userIdentifier: item.UserIdentifier,
				title: item.Title,
				author: item.Author,
				summary: item.Summary,
				publishedTimestamp: item.PublishedTimestamp,
				modifiedTimestamp: item.ModifiedTimestamp,
				url: item.Url,
				contentUrl: item.ContentUrl,
				contentLoader: new ItemContentLoader(_databaseService, _itemContentCache, _provider, item.Identifier),
				isRead: item.IsRead));
		foreach (var item in items)
		{
			RegisterItem(item);
		}
	}

	private async Task LoadDeletedItemsAsync(AppDbContext database)
	{
		var providerIdentifier = _provider.Metadata.Identifier;
		var identifiers = await database.DeletedItems
			.Where(item => item.ProviderIdentifier == providerIdentifier && item.StorageIdentifier == Identifier)
			.Select(item => item.UserIdentifier)
			.ToListAsync();
		_deletedIdentifiers = identifiers.ToHashSet();
	}

	private async Task LoadFeedItemsAsync(AppDbContext database)
	{
		var providerIdentifier = _provider.Metadata.Identifier;
		var feeds = await database.Feeds
			.Where(feed => feed.ProviderIdentifier == providerIdentifier && feed.ItemStorageIdentifier == Identifier)
			.Select(feed => feed.Identifier)
			.ToListAsync();
		foreach (var feed in feeds)
		{
			var items = new HashSet<Item>();
			var identifiers = await database.FeedItems
				.Where(item => item.FeedIdentifier == feed)
				.Select(item => item.ItemIdentifier)
				.ToListAsync();
			foreach (var identifier in identifiers)
			{
				if (_items.TryGetValue(identifier, out var item))
				{
					items.Add(item);
				}
			}
			_feedItems.Add(feed, items);
		}
	}
	
	private Task InitializeAsync()
	{
		return _databaseService.ExecuteAsync(
			async database =>
			{
				await LoadItemsAsync(database);
				await LoadDeletedItemsAsync(database);
				await LoadFeedItemsAsync(database);
			});
	}
	
	private readonly IDatabaseService _databaseService;
	private readonly ItemContentCache _itemContentCache;
	private readonly FeedProvider _provider;
	private readonly Dictionary<Guid, HashSet<Item>> _feedItems = new();
	private readonly Dictionary<Guid, Item> _items = new();
	private readonly Dictionary<string, Item> _identifierItems = new();
	private readonly Lazy<Task> _initialize;
	private HashSet<string> _deletedIdentifiers = new();
}
