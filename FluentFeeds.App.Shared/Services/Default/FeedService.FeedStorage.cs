using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentFeeds.App.Shared.Models.Database;
using FluentFeeds.Feeds.Base.Items;
using FluentFeeds.Feeds.Base.Items.Content;
using FluentFeeds.Feeds.Base.Items.ContentLoaders;
using FluentFeeds.Feeds.Base.Storage;
using Microsoft.EntityFrameworkCore;

namespace FluentFeeds.App.Shared.Services.Default;

public partial class FeedService
{
	/// <summary>
	/// Content loader which lazily loads the serialized content from the database and then acts as a proxy for the
	/// deserialized loader.
	/// </summary>
	private sealed class ItemContentLoader : IItemContentLoader
	{
		public ItemContentLoader(FeedService feedService, Guid identifier, IItemContentSerializer contentSerializer)
		{
			_feedService = feedService;
			_identifier = identifier;
			_contentSerializer = contentSerializer;
			_fetchFromDatabase = new Lazy<Task<IItemContentLoader>>(FetchFromDatabaseAsync, isThreadSafe: false);
		}
		
		private Task<IItemContentLoader> FetchFromDatabaseAsync()
		{
			return _feedService._databaseService.ExecuteAsync(
				async database =>
				{
					var serialized = await database.Items
						.Where(i => i.Identifier == _identifier)
						.Select(i => i.Content)
						.FirstAsync();
					return await _contentSerializer.LoadAsync(serialized);
				});
		}
		
		public Task<ItemContent> LoadAsync(bool reload = false)
		{
			return Task.Run(
				async () =>
				{
					var loader = await _fetchFromDatabase.Value;
					return await loader.LoadAsync(reload);
				});
		}

		private readonly FeedService _feedService;
		private readonly Guid _identifier;
		private readonly IItemContentSerializer _contentSerializer;
		private readonly Lazy<Task<IItemContentLoader>> _fetchFromDatabase;
	}
	
	private sealed class ItemStorage : IItemStorage
	{
		public ItemStorage(
			FeedService feedService, Guid providerIdentifier, Guid storageIdentifier,
			IItemContentSerializer contentSerializer)
		{
			_feedService = feedService;
			_providerIdentifier = providerIdentifier;
			_storageIdentifier = storageIdentifier;
			_contentSerializer = contentSerializer;

			_initialize = new Lazy<Task>(InitializeAsync);
		}

		/// <summary>
		/// Add an item to the cache.
		/// </summary>
		private void RegisterItem(StoredItem item)
		{
			_feedService._items.Add(item.Identifier, item);
			if (item.Url != null)
				_urlItems.Add(item.Url, item);
			_items.Add(item);
		}
		
		/// <summary>
		/// Store a collection of items in the database.
		/// </summary>
		private async Task StoreItemsAsync(AppDbContext database, IReadOnlyCollection<StoredItem> items)
		{
			var dbItems = new List<ItemDb> { Capacity = items.Count };
			foreach (var item in items)
			{
				dbItems.Add(
					new ItemDb
					{ 
						Identifier = item.Identifier, 
						ProviderIdentifier = _providerIdentifier,
						StorageIdentifier = _storageIdentifier,
						Url = item.Url,
						ContentUrl = item.ContentUrl,
						PublishedTimestamp = item.PublishedTimestamp,
						ModifiedTimestamp = item.ModifiedTimestamp,
						Title = item.Title,
						Author = item.Author,
						Summary = item.Summary,
						Content = await _contentSerializer.StoreAsync(item.ContentLoader),
						IsRead = item.IsRead
					});
			}
			
			await database.Items.AddRangeAsync(dbItems);
		}

		/// <summary>
		/// Update a set of items in the database. This does not update the item objects.
		/// </summary>
		private async Task UpdateItemsAsync(AppDbContext database, IEnumerable<StoredItem> items)
		{
			foreach (var item in items)
			{
				var identifier = item.Identifier;
				var dbItem = await database.Items.Where(i => i.Identifier == identifier).FirstAsync();
				dbItem.ContentUrl = item.ContentUrl;
				dbItem.ModifiedTimestamp = item.ModifiedTimestamp;
				dbItem.Title = item.Title;
				dbItem.Author = item.Author;
				dbItem.Summary = item.Summary;
				dbItem.Content = await _contentSerializer.StoreAsync(item.ContentLoader);
			}
		}

		/// <summary>
		/// Load all items in this storage from the database into stored item models.
		/// </summary>
		/// <param name="database"></param>
		/// <returns></returns>
		private async Task<List<StoredItem>> LoadItemsAsync(AppDbContext database)
		{
			var dbItems = await database.Items
				.Where(i =>
					i.ProviderIdentifier == _providerIdentifier && i.StorageIdentifier == _storageIdentifier)
				.Select(i =>
					new
					{
						i.Identifier,
						i.Url,
						i.ContentUrl,
						i.PublishedTimestamp,
						i.ModifiedTimestamp,
						i.Title,
						i.Author,
						i.Summary,
						i.IsRead
					})
				.ToListAsync();
			return dbItems
				.Select(item => new StoredItem(
					identifier: item.Identifier,
					url: item.Url,
					contentUrl: item.ContentUrl,
					publishedTimestamp: item.PublishedTimestamp,
					modifiedTimestamp: item.ModifiedTimestamp,
					title: item.Title,
					author: item.Author,
					summary: item.Summary,
					contentLoader: new ItemContentLoader(_feedService, item.Identifier, _contentSerializer),
					isRead: item.IsRead))
				.ToList();
		}

		private async Task InitializeAsync()
		{
			foreach (var item in await _feedService._databaseService.ExecuteAsync(LoadItemsAsync))
			{
				RegisterItem(item);
			}
		}

		public async Task<IEnumerable<IReadOnlyStoredItem>> GetItemsAsync()
		{
			await _initialize.Value;
			return _items;
		}

		public async Task<IEnumerable<IReadOnlyStoredItem>> AddItemsAsync(IEnumerable<IReadOnlyItem> items)
		{
			await _initialize.Value;
			
			var added = new List<StoredItem>();
			var updated = new List<StoredItem>();
			var result = new List<IReadOnlyStoredItem>();
			foreach (var item in items)
			{
				// Check if there is an item in this storage with this URL.
				if (item.Url != null && _urlItems.TryGetValue(item.Url, out var existing))
				{
					// Check if the item needs to be updated.
					if (existing.ModifiedTimestamp < item.ModifiedTimestamp)
					{
						existing.ContentUrl = item.ContentUrl;
						existing.ModifiedTimestamp = item.ModifiedTimestamp;
						existing.Title = item.Title;
						existing.Author = item.Author;
						existing.Summary = item.Summary;
						existing.ContentLoader = item.ContentLoader;
						updated.Add(new StoredItem(existing));
					}
					result.Add(existing);
				}
				else
				{
					// This is a new item.
					var storedItem = new StoredItem(item, Guid.NewGuid(), isRead: false);
					RegisterItem(storedItem);
					added.Add(new StoredItem(storedItem));
					result.Add(storedItem);
				}
			}

			await _feedService._databaseService.ExecuteAsync(
				async database =>
				{
					await UpdateItemsAsync(database, updated);
					await StoreItemsAsync(database, added);
					await database.SaveChangesAsync();
				});

			return result;
		}

		private readonly FeedService _feedService;
		private readonly Guid _providerIdentifier;
		private readonly Guid _storageIdentifier;
		private readonly IItemContentSerializer _contentSerializer;
		private readonly Dictionary<Uri, StoredItem> _urlItems = new();
		private readonly List<StoredItem> _items = new();
		private readonly Lazy<Task> _initialize;
	}
	
	private sealed class FeedStorage : IFeedStorage
	{
		public FeedStorage(FeedService feedService, Guid providerIdentifier)
		{
			_feedService = feedService;
			_providerIdentifier = providerIdentifier;
		}

		public IItemStorage GetItemStorage(Guid identifier, IItemContentSerializer? contentSerializer = null) =>
			_itemStorages.GetOrAdd(identifier, _ => CreateItemStorage(identifier, contentSerializer));

		private ItemStorage CreateItemStorage(Guid identifier, IItemContentSerializer? contentSerializer) =>
			new(_feedService, _providerIdentifier, identifier, contentSerializer ?? new DefaultItemContentSerializer());

		private readonly FeedService _feedService;
		private readonly Guid _providerIdentifier;
		private readonly ConcurrentDictionary<Guid, ItemStorage> _itemStorages = new();
	}
}
