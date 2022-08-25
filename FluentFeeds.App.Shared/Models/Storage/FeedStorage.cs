using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using FluentFeeds.App.Shared.EventArgs;
using FluentFeeds.App.Shared.Models.Database;
using FluentFeeds.App.Shared.Models.Feeds;
using FluentFeeds.App.Shared.Models.Feeds.Loaders;
using FluentFeeds.App.Shared.Services;
using FluentFeeds.Common;
using FluentFeeds.Feeds.Base;
using FluentFeeds.Feeds.Base.Feeds;
using FluentFeeds.Feeds.Base.Feeds.Content;
using Microsoft.EntityFrameworkCore;

namespace FluentFeeds.App.Shared.Models.Storage;

/// <summary>
/// Default feed storage implementation.
/// </summary>
public sealed class FeedStorage : IFeedStorage
{
	private sealed class StoredFeed : Feed
	{
		public StoredFeed(
			Guid identifier, IFeedStorage? storage, Func<IFeedView, FeedLoader> loaderFactory,
			bool hasChildren, IFeedView? parent, string? name, Symbol? symbol, FeedMetadata metadata,
			bool isUserCustomizable, bool isExcludedFromGroup)
			: base(
				identifier, storage, loaderFactory, hasChildren, parent, name, symbol, metadata, isUserCustomizable,
				isExcludedFromGroup)
		{
		}

		public FeedDb Db { get; set; } = null!;
	}
	
	public FeedStorage(IDatabaseService databaseService, ItemContentCache itemContentCache, FeedProvider provider)
	{
		_databaseService = databaseService;
		_itemContentCache = itemContentCache;
		Provider = provider;
		Identifier = Provider.Metadata.Identifier;
	}
	
	public FeedProvider Provider { get; }
	
	private Guid Identifier { get; }
	
	public event EventHandler<FeedsDeletedEventArgs>? FeedsDeleted;

	/// <summary>
	/// Load a feed and all of its children from its database representation.
	/// </summary>
	private async Task<StoredFeed> LoadFeedAsync(
		AppDbContext database, FeedDb dbFeed, IFeedView? parent, ICollection<StoredFeed>? allFeeds = null)
	{
		// Create loader factory
		Func<IFeedView, FeedLoader> loaderFactory;
		switch (dbFeed.Type)
		{
			case FeedDescriptorType.Group:
				loaderFactory = CreateGroupFeedLoader;
				break;
			case FeedDescriptorType.Cached:
				var contentLoader = await Provider.LoadFeedAsync(dbFeed.ContentLoader ?? String.Empty);
				var loader = new CachedFeedLoader(
					dbFeed.Identifier, GetItemStorage(dbFeed.ItemStorageIdentifier), contentLoader);
				loaderFactory = _ => loader;
				break;
			default:
				throw new IndexOutOfRangeException();
		}
		
		// Create the local feed object
		var feed = new StoredFeed(
			identifier: dbFeed.Identifier,
			storage: this,
			loaderFactory: loaderFactory,
			hasChildren: dbFeed.HasChildren,
			parent: parent,
			name: dbFeed.Name,
			symbol: dbFeed.Symbol,
			metadata: new FeedMetadata
			{
				Name = dbFeed.MetadataName,
				Symbol = dbFeed.MetadataSymbol,
				Author = dbFeed.MetadataAuthor,
				Description = dbFeed.MetadataDescription
			},
			isUserCustomizable: dbFeed.IsUserCustomizable,
			isExcludedFromGroup: dbFeed.IsExcludedFromGroup);
		feed.Db = dbFeed;
		allFeeds?.Add(feed);

		// Add children (sorted)
		if (feed.Children != null)
		{
			var dbChildren = await database.Feeds.Where(n => n.Parent == dbFeed).ToListAsync();
			var children = new List<StoredFeed>();
			foreach (var child in dbChildren)
			{
				children.Add(await LoadFeedAsync(database, child, feed, allFeeds));
			}

			var sortedChildren = children.OrderBy(f => f.DisplayName, StringComparer.CurrentCultureIgnoreCase);
			foreach (var child in sortedChildren)
			{
				feed.Children.Add(child);
			}
		}
		
		return feed;
	}

	private async Task<StoredFeed> CreateFeedAsync(FeedDescriptor descriptor, StoredFeed? parent, bool syncFirst)
	{
		// Create loader factory
		var identifier = Guid.NewGuid();
		var itemStorageIdentifier = identifier;
		string? contentLoader = null;
		FeedMetadata? initialMetadata = null;
		Func<IFeedView, FeedLoader> loaderFactory;
		ImmutableArray<FeedDescriptor> childDescriptors;
		switch (descriptor)
		{
			case GroupFeedDescriptor groupDescriptor:
				loaderFactory = CreateGroupFeedLoader;
				childDescriptors = groupDescriptor.Children;
				break;
			case CachedFeedDescriptor cachedDescriptor:
				if (cachedDescriptor.ItemCacheIdentifier.HasValue)
				{
					itemStorageIdentifier = cachedDescriptor.ItemCacheIdentifier.Value;
				}
				contentLoader = await Provider.StoreFeedAsync(cachedDescriptor.ContentLoader);
				childDescriptors = ImmutableArray<FeedDescriptor>.Empty;
				var loader = new CachedFeedLoader(
					identifier, GetItemStorage(itemStorageIdentifier), cachedDescriptor.ContentLoader);
				if (syncFirst)
				{
					await loader.SynchronizeAsync();
					initialMetadata = loader.Metadata;
				}
				loaderFactory = _ => loader;
				break;
			default:
				throw new IndexOutOfRangeException();
		}

		// Create the local feed object
		var feed = new StoredFeed(
			identifier: identifier,
			storage: this,
			loaderFactory: loaderFactory,
			hasChildren: descriptor.Type == FeedDescriptorType.Group,
			parent: parent,
			name: descriptor.Name,
			symbol: descriptor.Symbol,
			metadata: initialMetadata ?? new FeedMetadata(),
			isUserCustomizable: descriptor.IsUserCustomizable,
			isExcludedFromGroup: descriptor.IsExcludedFromGroup);
		feed.Db =
			new FeedDb
			{
				Identifier = feed.Identifier,
				ProviderIdentifier = Identifier,
				ItemStorageIdentifier = itemStorageIdentifier,
				Parent = parent?.Db,
				HasChildren = feed.Children != null,
				Type = descriptor.Type,
				ContentLoader = contentLoader,
				Name = feed.Name,
				Symbol = feed.Symbol,
				MetadataName = feed.Metadata.Name,
				MetadataSymbol = feed.Metadata.Symbol,
				MetadataAuthor = feed.Metadata.Author,
				MetadataDescription = feed.Metadata.Description,
				IsUserCustomizable = feed.IsUserCustomizable,
				IsExcludedFromGroup = feed.IsExcludedFromGroup
			};

		// Add children (sorted)
		if (feed.Children != null)
		{
			var children = new List<StoredFeed> { Capacity = childDescriptors.Length };
			foreach (var childDescriptor in childDescriptors)
			{
				children.Add(await CreateFeedAsync(childDescriptor, feed, syncFirst));
			}
			var sortedChildren = children.OrderBy(f => f.DisplayName, StringComparer.CurrentCultureIgnoreCase);
			foreach (var child in sortedChildren)
			{
				feed.Children.Add(child);
			}
		}

		return feed;
	}

	/// <summary>
	/// Store a new feed and all its children, returning its stored representation.
	/// </summary>
	private async Task<StoredFeed> StoreFeedAsync(
		AppDbContext database, StoredFeed feed, ICollection<StoredFeed>? allFeeds = null)
	{
		// Determine deleted items.
		var stack = new Stack<StoredFeed>();
		stack.Push(feed);
		while (stack.TryPop(out var currentFeed))
		{
			await database.Feeds.AddAsync(currentFeed.Db);
			allFeeds?.Add(currentFeed);

			if (currentFeed.Children != null)
			{
				foreach (var child in currentFeed.Children)
				{
					stack.Push((StoredFeed)child);
				}
			}
		}

		return feed;
	}

	/// <summary>
	/// Initialize this storage, returning the root node.
	/// </summary>
	public async Task<IFeedView> InitializeAsync(AppDbContext database)
	{
		var existingRootNode = await database.FeedProviders
			.Where(p => p.Identifier == Identifier)
			.Select(p => p.RootNode)
			.FirstOrDefaultAsync();

		var allFeeds = new List<StoredFeed>();
		StoredFeed rootFeed;
		if (existingRootNode != null)
		{
			rootFeed = await LoadFeedAsync(database, existingRootNode, null, allFeeds);
		}
		else
		{
			// This is a new provider, let it initialize the tree and store it in the database
			var initialTree = Provider.CreateInitialTree();
			rootFeed = await CreateFeedAsync(initialTree, null, syncFirst: false);
			await StoreFeedAsync(database, rootFeed, allFeeds);
			// Map the provider to the node
			await database.FeedProviders.AddAsync(
				new FeedProviderDb { Identifier = Identifier, RootNode = rootFeed.Db });
		}

		// Add nodes to the cache (no need for synchronization as this method is called before the storage is
		// exposed publicly.
		foreach (var feed in allFeeds)
		{
			_feeds.Add(feed.Identifier, feed);
		}
		
		return rootFeed;
	}
	
	public IItemStorage GetItemStorage(Guid identifier)
	{
		return _itemStorages.GetOrAdd(identifier, _ =>
			new ItemStorage(_databaseService, _itemContentCache, Provider, identifier));
	}

	public IFeedView? GetFeed(Guid identifier)
	{
		return _feeds.GetValueOrDefault(identifier);
	}

	public async Task<IFeedView> AddFeedAsync(FeedDescriptor descriptor, Guid parentIdentifier, bool syncFirst = false)
	{
		var parentFeed = _feeds[parentIdentifier];
		if (parentFeed.Children == null)
			throw new Exception("Invalid parent feed type.");

		var feed = await Task.Run(() => CreateFeedAsync(descriptor, parentFeed, syncFirst));

		// Update database
		await _databaseService.ExecuteAsync(
			async database =>
			{
				database.Attach(parentFeed.Db);
				await StoreFeedAsync(database, feed);
				await database.SaveChangesAsync();
			});

		// Update local copy
		_feeds.Add(feed.Identifier, feed);
		AddSorted(feed, parentFeed.Children);

		return feed;
	}

	public async Task<IFeedView> RenameFeedAsync(Guid identifier, string newName)
	{
		var feed = _feeds[identifier];

		// Update database
		await _databaseService.ExecuteAsync(
			async database =>
			{
				database.Attach(feed.Db);
				feed.Db.Name = newName;
				await database.SaveChangesAsync();
			});

		// Update local representation
		feed.Name = newName;

		return feed;
	}

	public async Task<IFeedView> MoveFeedAsync(Guid identifier, Guid newParentIdentifier)
	{
		var feed = _feeds[identifier];
		if (feed.Parent?.Identifier == newParentIdentifier)
			return feed;
		var newParent = _feeds[newParentIdentifier];
		if (newParent.Children == null)
			throw new Exception("Invalid parent feed type.");

		// Update database
		await _databaseService.ExecuteAsync(
			async database =>
			{
				database.Attach(feed.Db);
				feed.Db.Parent = newParent.Db;
				await database.SaveChangesAsync();
			});

		// Update local copy
		(feed.Parent as StoredFeed)?.Children?.Remove(feed);
		feed.Parent = newParent;
		AddSorted(feed, newParent.Children);

		return feed;
	}

	public async Task<IFeedView> UpdateFeedMetadataAsync(Guid identifier, FeedMetadata newMetadata)
	{
		var feed = _feeds[identifier];

		// Update database
		await _databaseService.ExecuteAsync(
			async database =>
			{
				database.Attach(feed.Db);
				feed.Db.MetadataName = newMetadata.Name;
				feed.Db.MetadataSymbol = newMetadata.Symbol;
				feed.Db.MetadataAuthor = newMetadata.Author;
				feed.Db.MetadataDescription = newMetadata.Description;
				await database.SaveChangesAsync();
			});

		// Update local representation
		feed.Metadata = newMetadata;

		return feed;
	}

	public async Task DeleteFeedAsync(Guid identifier)
	{
		var feed = _feeds[identifier];
			
		// Determine deleted items.
		var stack = new Stack<StoredFeed>();
		var deleted = new List<StoredFeed>();
		stack.Push(feed);
		while (stack.TryPop(out var currentFeed))
		{
			deleted.Add(currentFeed);

			if (currentFeed.Children != null)
			{
				foreach (var child in currentFeed.Children)
				{
					stack.Push((StoredFeed)child);
				}
			}
		}

		// Update database
		await _databaseService.ExecuteAsync(
			async database =>
			{
				var dbNodes = deleted.Select(n => n.Db).ToList();
				database.AttachRange(dbNodes);
				database.RemoveRange(dbNodes);
				await database.SaveChangesAsync();
			});

		// Update local representation
		(feed.Parent as StoredFeed)?.Children?.Remove(feed);
		FeedsDeleted?.Invoke(this, new FeedsDeletedEventArgs(deleted));
		foreach (var deletedNode in deleted)
		{
			_feeds.Remove(deletedNode.Identifier);
		}
	}
	
	private static FeedLoader CreateGroupFeedLoader(IFeedView feed)
	{
		ImmutableHashSet<FeedLoader> GetLoaders()
		{
			return feed.Children?
				.Where(f => !f.IsExcludedFromGroup)
				.Select(f => f.Loader)
				.ToImmutableHashSet() ?? ImmutableHashSet<FeedLoader>.Empty;
		}

		var loader = new GroupFeedLoader(GetLoaders());
		if (feed.Children != null)
		{
			(feed.Children as INotifyCollectionChanged).CollectionChanged += (s, e) => loader.Loaders = GetLoaders();
		}

		return loader;
	}
	
	/// <summary>
	/// Add a feed to a collection, ensuring that the collection remains sorted.
	/// </summary>
	private static void AddSorted(IFeedView feed, IList<IFeedView> container)
	{
		for (var i = 0; i < container.Count; ++i)
		{
			var comparisonResult = String.Compare(
				container[i].DisplayName, feed.DisplayName, StringComparison.CurrentCultureIgnoreCase);
			if (comparisonResult > 0)
			{
				container.Insert(i, feed);
				return;
			}
		}

		container.Add(feed);
	}

	private readonly IDatabaseService _databaseService;
	private readonly ItemContentCache _itemContentCache;
	private readonly Dictionary<Guid, StoredFeed> _feeds = new();
	private readonly ConcurrentDictionary<Guid, ItemStorage> _itemStorages = new();
}
