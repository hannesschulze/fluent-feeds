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
	private sealed class ItemContentLoader : IItemContentLoader
	{
		public ItemContentLoader(ItemStorage storage, Guid identifier)
		{
			Storage = storage;
			Identifier = identifier;
			_fetchFromDatabase = new Lazy<Task<IItemContentLoader>>(FetchFromDatabaseAsync, isThreadSafe: false);
		}
		
		public ItemStorage Storage { get; }
		public Guid Identifier { get; }
		
		private async Task<IItemContentLoader> FetchFromDatabaseAsync()
		{
			// TODO: FeedService.WithDatabase(async database => { … });
			await using var database = Storage.FeedService.DatabaseService.CreateContext();
			var serialized = await database.Items
				.Where(i => i.Identifier == Identifier)
				.Select(i => i.Content)
				.FirstAsync();
			return await Storage.ContentSerializer.LoadAsync(serialized);
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

		private readonly Lazy<Task<IItemContentLoader>> _fetchFromDatabase;
	}
	
	private sealed class ItemStorage : IItemStorage
	{
		public ItemStorage(
			FeedService feedService, Guid providerIdentifier, Guid storageIdentifier,
			IItemContentSerializer contentSerializer)
		{
			FeedService = feedService;
			ProviderIdentifier = providerIdentifier;
			StorageIdentifier = storageIdentifier;
			ContentSerializer = contentSerializer;

			_loadItems = new Lazy<Task>(LoadItemsAsync);
		}
		
		public FeedService FeedService { get; }
		public Guid ProviderIdentifier { get; }
		public Guid StorageIdentifier { get; }
		public IItemContentSerializer ContentSerializer { get; }

		private void RegisterItem(StoredItem item)
		{
			FeedService._items.Add(item.Identifier, item);
			if (item.Url != null)
				_urlItems.Add(item.Url, item);
			_items.Add(item);
		}
		
		private async Task StoreItemsAsync(AppDbContext database, IReadOnlyCollection<StoredItem> items)
		{
			var dbItems = new List<ItemDb> { Capacity = items.Count };
			foreach (var item in items)
			{
				dbItems.Add(
					new ItemDb
					{ 
						Identifier = item.Identifier, 
						ProviderIdentifier = ProviderIdentifier,
						StorageIdentifier = StorageIdentifier,
						Url = item.Url,
						ContentUrl = item.ContentUrl,
						PublishedTimestamp = item.PublishedTimestamp,
						ModifiedTimestamp = item.ModifiedTimestamp,
						Title = item.Title,
						Author = item.Author,
						Summary = item.Summary,
						Content = await ContentSerializer.StoreAsync(item.ContentLoader),
						IsRead = item.IsRead
					});
			}
			
			await database.Items.AddRangeAsync(dbItems);
		}

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
				dbItem.Content = await ContentSerializer.StoreAsync(item.ContentLoader);
			}
		}

		private async Task LoadItemsAsync()
		{
			var items = new List<StoredItem>();
			// TODO: FeedService.WithDatabase(async database => { … });
			await Task.Run(
				async () =>
				{
					await using var database = FeedService.DatabaseService.CreateContext();
					var dbItems = await database.Items
						.Where(i =>
							i.ProviderIdentifier == ProviderIdentifier && i.StorageIdentifier == StorageIdentifier)
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
					items.EnsureCapacity(dbItems.Count);
					foreach (var item in dbItems)
					{
						items.Add(new StoredItem(
							identifier: item.Identifier,
							url: item.Url,
							contentUrl: item.ContentUrl,
							publishedTimestamp: item.PublishedTimestamp,
							modifiedTimestamp: item.ModifiedTimestamp,
							title: item.Title,
							author: item.Author,
							summary: item.Summary,
							contentLoader: new ItemContentLoader(this, item.Identifier),
							isRead: item.IsRead));
					}
				});
			foreach (var item in items)
			{
				RegisterItem(item);
			}
		}

		public async Task<IEnumerable<IReadOnlyStoredItem>> GetItemsAsync()
		{
			await _loadItems.Value;
			return _items;
		}

		public async Task<IEnumerable<IReadOnlyStoredItem>> AddItemsAsync(IEnumerable<IReadOnlyItem> items)
		{
			await _loadItems.Value;
			
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

			// TODO: FeedService.WithDatabase(async database => { … });
			await Task.Run(
				async () =>
				{
					await using var database = FeedService.DatabaseService.CreateContext();
					await UpdateItemsAsync(database, updated);
					await StoreItemsAsync(database, added);
					await database.SaveChangesAsync();
				});

			return result;
		}

		private readonly Dictionary<Uri, StoredItem> _urlItems = new();
		private readonly List<StoredItem> _items = new();
		private readonly Lazy<Task> _loadItems;
	}
	
	private sealed class FeedStorage : IFeedStorage
	{
		public FeedStorage(FeedService feedService, Guid providerIdentifier)
		{
			FeedService = feedService;
			ProviderIdentifier = providerIdentifier;
		}
		
		public FeedService FeedService { get; }
		public Guid ProviderIdentifier { get; }

		public IItemStorage GetItemStorage(Guid identifier, IItemContentSerializer? contentSerializer = null) =>
			_itemStorages.GetOrAdd(identifier, _ => new ItemStorage(
				FeedService, ProviderIdentifier, identifier, contentSerializer ?? new DefaultItemContentSerializer()));

		private readonly ConcurrentDictionary<Guid, ItemStorage> _itemStorages = new();
	}
}
