using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using FluentFeeds.App.Shared.Models;
using FluentFeeds.App.Shared.Models.Database;
using FluentFeeds.Common;
using FluentFeeds.Documents;
using FluentFeeds.Feeds.Base;
using FluentFeeds.Feeds.Base.Items;
using FluentFeeds.Feeds.Base.Items.Content;
using FluentFeeds.Feeds.Base.Nodes;
using FluentFeeds.Feeds.Base.Storage;
using Microsoft.EntityFrameworkCore;

namespace FluentFeeds.App.Shared.Services.Default;

public class FeedService : IFeedService
{
	private sealed class ItemStorage : IItemStorage
	{
		public ItemStorage(FeedService feedService, Guid providerIdentifier, Guid storageIdentifier)
		{
			_feedService = feedService;
			_providerIdentifier = providerIdentifier;
			_storageIdentifier = storageIdentifier;
		}
		
		private static async Task UpdateItemAsync(AppDbContext context, StoredItem item, IReadOnlyItem updated)
		{
			var identifier = item.Identifier;
			var dbItem = await context.Items.Where(i => i.Identifier == identifier).FirstOrDefaultAsync();
			if (dbItem == null)
				return;
			
			dbItem.ContentUrl = item.ContentUrl = updated.ContentUrl;
			dbItem.ModifiedTimestamp = item.ModifiedTimestamp = updated.ModifiedTimestamp;
			dbItem.Title = item.Title = updated.Title;
			dbItem.Author = item.Author = updated.Author;
			dbItem.Summary = item.Summary = updated.Summary;
			dbItem.ContentType = updated.Content.Type;
			dbItem.ArticleContentBody = (item.Content as ArticleItemContent)?.Body;
			item.Content = updated.Content;
		}

		private void AddItemToCache(StoredItem item)
		{
			if (item.Url != null)
				_urlItems.Add(item.Url, item);
			_feedService._items.Add(item.Identifier, item);
			_items.Add(item);
		}

		private StoredItem LoadItem(ItemDb dbItem)
		{
			var content =
				dbItem.ContentType switch
				{
					ItemContentType.Article =>
						new ArticleItemContent(dbItem.ArticleContentBody ?? new RichText()),
					_ => throw new IndexOutOfRangeException()
				};
			var item = new StoredItem(
				identifier: dbItem.Identifier,
				url: dbItem.Url,
				contentUrl: dbItem.ContentUrl,
				publishedTimestamp: dbItem.PublishedTimestamp,
				modifiedTimestamp: dbItem.ModifiedTimestamp,
				title: dbItem.Title,
				author: dbItem.Author,
				summary: dbItem.Summary,
				content: content,
				isRead: dbItem.IsRead);
			AddItemToCache(item);
			return item;
		}

		private async Task<StoredItem> StoreItemAsync(AppDbContext database, IReadOnlyItem item)
		{
			var storedItem = new StoredItem(item, Guid.NewGuid(), isRead: false);
			await database.Items.AddAsync(
				new ItemDb
				{ 
					Identifier = storedItem.Identifier, 
					ProviderIdentifier = _providerIdentifier,
					StorageIdentifier = _storageIdentifier,
					Url = storedItem.Url,
					ContentUrl = storedItem.ContentUrl,
					PublishedTimestamp = storedItem.PublishedTimestamp,
					ModifiedTimestamp = storedItem.ModifiedTimestamp,
					Title = storedItem.Title,
					Author = storedItem.Author,
					Summary = storedItem.Summary,
					ContentType = storedItem.Content.Type,
					ArticleContentBody = (storedItem.Content as ArticleItemContent)?.Body,
					IsRead = storedItem.IsRead
				});
			AddItemToCache(storedItem);
			return storedItem;
		}

		private Task LoadItemsAsync()
		{
			if (_isLoaded)
				return Task.CompletedTask;
		
			_loadTask ??= LoadItemsAsyncCore();
			return _loadTask;
		}

		private async Task LoadItemsAsyncCore()
		{
			await using var database = _feedService._databaseService.CreateContext();
			var dbItems = await database.Items
				.Where(i => i.ProviderIdentifier == _providerIdentifier && i.StorageIdentifier == _storageIdentifier)
				.ToListAsync();
			_items.EnsureCapacity(dbItems.Count);
			foreach (var dbItem in dbItems)
			{
				LoadItem(dbItem);
			}

			_isLoaded = true;
		}

		public async Task<IEnumerable<IReadOnlyStoredItem>> GetItemsAsync()
		{
			await LoadItemsAsync();
			return _items;
		}

		public async Task<IEnumerable<IReadOnlyStoredItem>> AddItemsAsync(IEnumerable<IReadOnlyItem> items)
		{
			await LoadItemsAsync();
			await using var database = _feedService._databaseService.CreateContext();
			var result = new List<IReadOnlyStoredItem>();
			foreach (var item in items)
			{
				// Check if there is an item in this storage with this URL.
				if (item.Url != null && _urlItems.TryGetValue(item.Url, out var existing))
				{
					// Check if the item needs to be updated.
					if (existing.ModifiedTimestamp < item.ModifiedTimestamp)
						await UpdateItemAsync(database, existing, item);
					result.Add(existing);
				}
				else
				{
					// This is a new item
					result.Add(await StoreItemAsync(database, item));
				}
			}

			await database.SaveChangesAsync();

			return result;
		}

		private readonly FeedService _feedService;
		private readonly Guid _providerIdentifier;
		private readonly Guid _storageIdentifier;
		private Dictionary<Uri, StoredItem> _urlItems = new();
		private List<StoredItem> _items = new();
		private bool _isLoaded;
		private Task? _loadTask;
	}
	
	private sealed class FeedStorage : IFeedStorage
	{
		public FeedStorage(FeedService feedService, Guid providerIdentifier)
		{
			_feedService = feedService;
			_providerIdentifier = providerIdentifier;
		}
		
		public IItemStorage GetItemStorage(Guid identifier)
		{
			if (_itemStorages.TryGetValue(identifier, out var existing))
				return existing;

			var itemStorage = new ItemStorage(_feedService, _providerIdentifier, identifier);
			_itemStorages.Add(identifier, itemStorage);
			return itemStorage;
		}

		private readonly FeedService _feedService;
		private readonly Guid _providerIdentifier;
		private readonly Dictionary<Guid, ItemStorage> _itemStorages = new();
	}
	
	public FeedService(IDatabaseService databaseService, IPluginService pluginService)
	{
		_databaseService = databaseService;
		_pluginService = pluginService;

		FeedProviders = new ReadOnlyObservableCollection<LoadedFeedProvider>(_feedProviders);
		OverviewFeed = FeedNode.Custom(_overviewFeed, "Overview", Symbol.Home, isUserCustomizable: false);
	}
	
	public Task LoadFeedProvidersAsync()
	{
		if (_feedProvidersLoaded)
			return Task.CompletedTask;
		
		_loadFeedProvidersTask ??= LoadFeedProvidersAsyncCore();
		return _loadFeedProvidersTask;
	}

	public ReadOnlyObservableCollection<LoadedFeedProvider> FeedProviders { get; }
	
	public IReadOnlyFeedNode OverviewFeed { get; }

	/// <summary>
	/// Load a feed node and all of its children from its database representation.
	/// </summary>
	private async Task<IReadOnlyStoredFeedNode> LoadFeedNodeAsync(
		AppDbContext database, FeedNodeDb node, FeedProvider provider, IFeedStorage storage)
	{
		List<IReadOnlyFeedNode>? children = null;
		if (node.HasChildren)
		{
			var dbChildren = await database.FeedNodes.Where(n => n.Parent == node).ToListAsync();
			children = new List<IReadOnlyFeedNode> { Capacity = dbChildren.Count };
			foreach (var child in dbChildren)
			{
				children.Add(await LoadFeedNodeAsync(database, child, provider, storage));
			}
		}
		
		var storedNode =
			node.Type switch
			{
				FeedNodeType.Group => StoredFeedNode.Group(
					node.Identifier, node.Title, node.Symbol, node.IsUserCustomizable,
					children ?? Enumerable.Empty<IReadOnlyFeedNode>()),
				FeedNodeType.Custom => StoredFeedNode.Custom(
					node.Identifier, provider.LoadFeed(storage, node.CustomSerialized ?? String.Empty), node.Title,
					node.Symbol, node.IsUserCustomizable, children),
				_ => throw new IndexOutOfRangeException()
			};
		_feedNodes.Add(storedNode.Identifier, storedNode);
		return storedNode;
	}

	/// <summary>
	/// Store a new feed node and all its children, returning its stored representation.
	/// </summary>
	private async Task<(IReadOnlyStoredFeedNode Stored, FeedNodeDb Db)> StoreFeedNodeAsync(
		AppDbContext database, IReadOnlyFeedNode node, FeedProvider provider, FeedNodeDb? parent)
	{
		var identifier = Guid.NewGuid();
		var dbNode =
			new FeedNodeDb
			{
				Identifier = identifier,
				Parent = parent,
				HasChildren = node.Children != null,
				Type = node.Type,
				CustomSerialized = node.Type == FeedNodeType.Custom ? provider.StoreFeed(node.Feed) : null,
				Title = node.Title,
				Symbol = node.Symbol,
				IsUserCustomizable = node.IsUserCustomizable
			};
		await database.FeedNodes.AddAsync(dbNode);

		List<IReadOnlyFeedNode>? children = null;
		if (node.Children != null)
		{
			children = new List<IReadOnlyFeedNode> { Capacity = node.Children.Count };
			foreach (var child in node.Children)
			{
				var (storedChild, _) = await StoreFeedNodeAsync(database, child, provider, dbNode);
				children.Add(storedChild);
			}
		}
		
		var storedNode =
			node.Type switch
			{
				FeedNodeType.Group => StoredFeedNode.Group(
					identifier, node.Title, node.Symbol, node.IsUserCustomizable,
					children ?? Enumerable.Empty<IReadOnlyFeedNode>()),
				FeedNodeType.Custom => StoredFeedNode.Custom(
					identifier, node.Feed, node.Title, node.Symbol, node.IsUserCustomizable, children),
				_ => throw new IndexOutOfRangeException()
			};
		_feedNodes.Add(identifier, storedNode);
		return (storedNode, dbNode);
	}

	private async Task LoadFeedProvidersAsyncCore()
	{
		await _databaseService.InitializeAsync();
		await using var database = _databaseService.CreateContext();

		foreach (var provider in _pluginService.GetAvailableFeedProviders())
		{
			var identifier = provider.Metadata.Identifier;
			var existingRootNode = await database.FeedProviders
				.Where(p => p.Identifier == identifier)
				.Select(p => p.RootNode)
				.FirstOrDefaultAsync();
			var storage = new FeedStorage(this, identifier);
			
			IReadOnlyStoredFeedNode rootNode;
			if (existingRootNode != null)
			{
				rootNode = await LoadFeedNodeAsync(database, existingRootNode, provider, storage);
			}
			else
			{
				var (storedNode, dbNode) =
					await StoreFeedNodeAsync(database, provider.CreateInitialTree(), provider, null); 
				database.FeedProviders.Add(
					new FeedProviderDb { Identifier = provider.Metadata.Identifier, RootNode = dbNode });
				rootNode = storedNode;
			}

			_feedProviders.Add(new LoadedFeedProvider(
				Provider: provider, RootNode: rootNode, FeedStorage: storage));
		}

		await database.SaveChangesAsync();
		
		_overviewFeed.Feeds = FeedProviders.Select(provider => provider.RootNode.Feed).ToImmutableHashSet();
		
		_feedProvidersLoaded = true;
	}

	private readonly IDatabaseService _databaseService;
	private readonly IPluginService _pluginService;
	private readonly ObservableCollection<LoadedFeedProvider> _feedProviders = new();
	private readonly CompositeFeed _overviewFeed = new();
	private readonly Dictionary<Guid, StoredFeedNode> _feedNodes = new();
	private readonly Dictionary<Guid, StoredItem> _items = new();
	private bool _feedProvidersLoaded;
	private Task? _loadFeedProvidersTask;
}
