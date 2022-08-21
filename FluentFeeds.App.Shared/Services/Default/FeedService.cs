using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using AngleSharp.Dom;
using FluentFeeds.App.Shared.Models;
using FluentFeeds.App.Shared.Models.Database;
using FluentFeeds.Common;
using FluentFeeds.Feeds.Base;
using FluentFeeds.Feeds.Base.Items;
using FluentFeeds.Feeds.Base.Nodes;
using FluentFeeds.Feeds.Base.Storage;
using Microsoft.EntityFrameworkCore;

namespace FluentFeeds.App.Shared.Services.Default;

public partial class FeedService : IFeedService
{
	private sealed class CachedFeedNode : StoredFeedNode
	{
		public CachedFeedNode(IReadOnlyFeedNode node, FeedProvider provider, CachedFeedNode? parent, FeedNodeDb db)
			: base(node, db.Identifier)
		{
			Parent = parent;
			Provider = provider;
			Db = db;
		}
		
		public CachedFeedNode? Parent { get; set; }
		public FeedProvider Provider { get; }
		public FeedNodeDb Db { get; }
	}

	public FeedService(IDatabaseService databaseService, IPluginService pluginService)
	{
		_databaseService = databaseService;
		_pluginService = pluginService;
		_initialize = new Lazy<Task>(InitializeAsyncCore);

		FeedProviders = new ReadOnlyObservableCollection<LoadedFeedProvider>(_feedProviders);
		OverviewFeed = FeedNode.Custom(_overviewFeed, "Overview", Symbol.Home, isUserCustomizable: false);
	}

	public Task InitializeAsync() => _initialize.Value;

	public async Task AddGroupNodeAsync(Guid parentIdentifier, string name)
	{
		var parentNode = _feedNodes[parentIdentifier];
		if (parentNode.Type != FeedNodeType.Group)
			throw new Exception("Invalid parent node type.");
		var newNode = FeedNode.Group(name, Symbol.Directory, isUserCustomizable: true);

		// Update database
		var node = await _databaseService.ExecuteAsync(
			async database =>
			{
				database.Attach(parentNode.Db);
				var node = await StoreFeedNodeAsync(database, newNode, parentNode.Provider, parentNode);
				await database.SaveChangesAsync();
				return node;
			});

		// Update local representation
		AddNodeSorted(node, parentNode.Children!);
		_feedNodes.Add(node.Identifier, node);
	}

	public async Task AddFeedNodeAsync(Guid parentIdentifier, Feed feed)
	{
		var parentNode = _feedNodes[parentIdentifier];
		if (parentNode.Type != FeedNodeType.Group)
			throw new Exception("Invalid parent node type.");
		var newNode = FeedNode.Custom(feed, null, null, isUserCustomizable: true);

		// Update database
		var node = await _databaseService.ExecuteAsync(
			async database =>
			{
				database.Attach(parentNode.Db);
				var node = await StoreFeedNodeAsync(database, newNode, parentNode.Provider, parentNode);
				await database.SaveChangesAsync();
				return node;
			});

		// Update local representation
		AddNodeSorted(node, parentNode.Children!);
		_feedNodes.Add(node.Identifier, node);
	}

	public async Task DeleteNodeAsync(Guid identifier)
	{
		// Determine deleted items.
		var node = _feedNodes[identifier];
		var stack = new Stack<CachedFeedNode>();
		var deleted = new List<CachedFeedNode>();
		stack.Push(node);
		while (stack.TryPop(out var currentNode))
		{
			deleted.Add(currentNode);

			if (currentNode.Children != null)
			{
				foreach (var child in currentNode.Children)
				{
					stack.Push((CachedFeedNode)child);
				}
			}
		}

		// Update database
		await _databaseService.ExecuteAsync(
			async database =>
			{
				var dbNodes = deleted.Select(node => node.Db).ToList();
				database.AttachRange(dbNodes);
				database.RemoveRange(dbNodes);
				await database.SaveChangesAsync();
			});

		// Update local representation
		node.Parent?.Children?.Remove(node);
		foreach (var deletedNode in deleted)
		{
			_feedNodes.Remove(deletedNode.Identifier);
		}
	}

	public IReadOnlyStoredFeedNode? GetNode(Guid identifier)
	{
		return _feedNodes.GetValueOrDefault(identifier);
	}

	public IReadOnlyStoredFeedNode? GetParentNode(Guid identifier)
	{
		return _feedNodes.GetValueOrDefault(identifier)?.Parent;
	}

	public async Task EditNodeAsync(Guid identifier, Guid parentIdentifier, string name)
	{
		var node = _feedNodes[identifier];
		var oldParent = node.Parent;
		var newParent = oldParent?.Identifier != parentIdentifier ? _feedNodes[parentIdentifier] : null;

		// Update database
		await _databaseService.ExecuteAsync(
			async database =>
			{
				database.Attach(node.Db);
				if (name != node.ActualTitle)
					node.Db.Title = name;
				if (newParent != null)
					node.Db.Parent = newParent.Db;
				await database.SaveChangesAsync();
			});

		// Update local representation
		if (name != node.Title)
		{
			node.Title = name;
		}

		if (newParent != null)
		{
			// Move the local item to the new parent.
			oldParent?.Children?.Remove(node);
			node.Parent = newParent;
			AddNodeSorted(node, newParent.Children!);
		}
	}

	private static void AddNodeSorted(CachedFeedNode node, ObservableCollection<IReadOnlyFeedNode> container)
	{
		for (var i = 0; i < container.Count; ++i)
		{
			var comparisonResult = String.Compare(
				container[i].ActualTitle, node.ActualTitle, StringComparison.CurrentCultureIgnoreCase);
			if (comparisonResult > 0)
			{
				container.Insert(i, node);
				return;
			}
		}

		container.Add(node);
	}

	public ReadOnlyObservableCollection<LoadedFeedProvider> FeedProviders { get; }
	
	public IReadOnlyFeedNode OverviewFeed { get; }

	/// <summary>
	/// Load a feed node and all of its children from its database representation.
	/// </summary>
	private static async Task<CachedFeedNode> LoadFeedNodeAsync(
		AppDbContext database, FeedNodeDb dbNode, FeedProvider provider, IFeedStorage storage, CachedFeedNode? parent,
		ICollection<CachedFeedNode>? allNodes = null)
	{
		var node = new CachedFeedNode(
			dbNode.Type switch
			{
				FeedNodeType.Group => FeedNode.Group(dbNode.Title, dbNode.Symbol, dbNode.IsUserCustomizable),
				FeedNodeType.Custom => FeedNode.Custom(
					await provider.LoadFeedAsync(storage, dbNode.Feed ?? String.Empty), dbNode.Title, dbNode.Symbol,
					dbNode.IsUserCustomizable, dbNode.HasChildren ? Enumerable.Empty<IReadOnlyFeedNode>() : null),
				_ => throw new IndexOutOfRangeException()
			}, provider, parent, dbNode);
		allNodes?.Add(node);

		if (node.Children != null)
		{
			var dbChildren = await database.FeedNodes.Where(n => n.Parent == dbNode).ToListAsync();
			var children = new List<CachedFeedNode>();
			foreach (var child in dbChildren)
			{
				children.Add(await LoadFeedNodeAsync(database, child, provider, storage, node, allNodes));
			}

			var sortedChildren = children.OrderBy(n => n.ActualTitle, StringComparer.CurrentCultureIgnoreCase);
			foreach (var child in sortedChildren)
			{
				node.Children.Add(child);
			}
		}

		return node;
	}

	/// <summary>
	/// Store a new feed node and all its children, returning its stored representation.
	/// </summary>
	private static async Task<CachedFeedNode> StoreFeedNodeAsync(
		AppDbContext database, IReadOnlyFeedNode inputNode, FeedProvider provider, CachedFeedNode? parent,
		ICollection<CachedFeedNode>? allNodes = null)
	{
		var node = new CachedFeedNode(
			inputNode, provider, parent,
			new FeedNodeDb
			{
				Identifier = Guid.NewGuid(),
				Parent = parent?.Db,
				HasChildren = inputNode.Children != null,
				Type = inputNode.Type,
				Feed = inputNode.Type == FeedNodeType.Custom ? await provider.StoreFeedAsync(inputNode.Feed) : null,
				Title = inputNode.Title,
				Symbol = inputNode.Symbol,
				IsUserCustomizable = inputNode.IsUserCustomizable
			});
		await database.FeedNodes.AddAsync(node.Db);
		allNodes?.Add(node);

		if (node.Children != null)
		{
			node.Children.Clear();
			var sortedChildren = inputNode.Children!
				.OrderBy(n => n.ActualTitle, StringComparer.CurrentCultureIgnoreCase);
			foreach (var child in sortedChildren)
			{
				node.Children.Add(await StoreFeedNodeAsync(database, child, provider, node, allNodes));
			}
		}

		return node;
	}

	/// <summary>
	/// Load a list of feed providers. This either stores the initial tree structure in the database if a feed provider
	/// is new or loads the existing structure from the database.
	/// </summary>
	private async Task<List<LoadedFeedProvider>> LoadFeedProvidersAsync(
		AppDbContext database, IEnumerable<FeedProvider> providers, ICollection<CachedFeedNode>? allNodes = null)
	{
		var result = new List<LoadedFeedProvider>();
		foreach (var provider in providers)
		{
			var identifier = provider.Metadata.Identifier;
			var existingRootNode = await database.FeedProviders
				.Where(p => p.Identifier == identifier)
				.Select(p => p.RootNode)
				.FirstOrDefaultAsync();
			var storage = new FeedStorage(this, identifier);

			CachedFeedNode rootNode;
			if (existingRootNode != null)
			{
				rootNode = await LoadFeedNodeAsync(database, existingRootNode, provider, storage, null, allNodes);
			}
			else
			{
				rootNode =
					await StoreFeedNodeAsync(database, provider.CreateInitialTree(storage), provider, null, allNodes);
				await database.FeedProviders.AddAsync(
					new FeedProviderDb { Identifier = provider.Metadata.Identifier, RootNode = rootNode.Db });
			}

			result.Add(new LoadedFeedProvider(provider, rootNode, storage));
		}

		await database.SaveChangesAsync();
		
		return result;
	}

	private async Task InitializeAsyncCore()
	{
		var providers = _pluginService.GetAvailableFeedProviders();
		var allNodes = new List<CachedFeedNode>();
		var loadedProviders =
			await _databaseService.ExecuteAsync(database => LoadFeedProvidersAsync(database, providers, allNodes));
		
		foreach (var provider in loadedProviders)
			_feedProviders.Add(provider);
		foreach (var node in allNodes)
			_feedNodes.Add(node.Identifier, node);
		_overviewFeed.Feeds = loadedProviders.Select(provider => provider.RootNode.Feed).ToImmutableHashSet();
	}

	private readonly IDatabaseService _databaseService;
	private readonly IPluginService _pluginService;
	private readonly Lazy<Task> _initialize;
	private readonly ObservableCollection<LoadedFeedProvider> _feedProviders = new();
	private readonly CompositeFeed _overviewFeed = new();
	private readonly Dictionary<Guid, CachedFeedNode> _feedNodes = new();
	private readonly Dictionary<Guid, StoredItem> _items = new();
}
