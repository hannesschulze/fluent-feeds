using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
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
	public FeedService(IDatabaseService databaseService, IPluginService pluginService)
	{
		_databaseService = databaseService;
		_pluginService = pluginService;
		_initialize = new Lazy<Task>(InitializeAsyncCore);

		FeedProviders = new ReadOnlyObservableCollection<LoadedFeedProvider>(_feedProviders);
		OverviewFeed = FeedNode.Custom(_overviewFeed, "Overview", Symbol.Home, isUserCustomizable: false);
	}

	public Task InitializeAsync() => _initialize.Value;

	public ReadOnlyObservableCollection<LoadedFeedProvider> FeedProviders { get; }
	
	public IReadOnlyFeedNode OverviewFeed { get; }

	/// <summary>
	/// Load a feed node and all of its children from its database representation.
	/// </summary>
	private static async Task<StoredFeedNode> LoadFeedNodeAsync(
		AppDbContext database, FeedNodeDb node, FeedProvider provider, IFeedStorage storage,
		ICollection<StoredFeedNode> allNodes)
	{
		List<IReadOnlyFeedNode>? children = null;
		if (node.HasChildren)
		{
			var dbChildren = await database.FeedNodes.Where(n => n.Parent == node).ToListAsync();
			children = new List<IReadOnlyFeedNode> { Capacity = dbChildren.Count };
			foreach (var child in dbChildren)
			{
				children.Add(await LoadFeedNodeAsync(database, child, provider, storage, allNodes));
			}
		}
		
		var storedNode =
			node.Type switch
			{
				FeedNodeType.Group => StoredFeedNode.Group(
					node.Identifier, node.Title, node.Symbol, node.IsUserCustomizable,
					children ?? Enumerable.Empty<IReadOnlyFeedNode>()),
				FeedNodeType.Custom => StoredFeedNode.Custom(
					node.Identifier, await provider.LoadFeedAsync(storage, node.Feed ?? String.Empty),
					node.Title, node.Symbol, node.IsUserCustomizable, children),
				_ => throw new IndexOutOfRangeException()
			};
		allNodes.Add(storedNode);
		return storedNode;
	}

	/// <summary>
	/// Store a new feed node and all its children, returning its stored representation.
	/// </summary>
	private static async Task<(StoredFeedNode Stored, FeedNodeDb Db)> StoreFeedNodeAsync(
		AppDbContext database, IReadOnlyFeedNode node, FeedProvider provider, FeedNodeDb? parent,
		ICollection<StoredFeedNode> allNodes)
	{
		var identifier = Guid.NewGuid();
		var dbNode =
			new FeedNodeDb
			{
				Identifier = identifier,
				Parent = parent,
				HasChildren = node.Children != null,
				Type = node.Type,
				Feed = node.Type == FeedNodeType.Custom ? await provider.StoreFeedAsync(node.Feed) : null,
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
				var (storedChild, _) = await StoreFeedNodeAsync(database, child, provider, dbNode, allNodes);
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
		allNodes.Add(storedNode);
		return (storedNode, dbNode);
	}

	/// <summary>
	/// Load a list of feed providers. This either stores the initial tree structure in the database if a feed provider
	/// is new or loads the existing structure from the database.
	/// </summary>
	private async Task<List<LoadedFeedProvider>> LoadFeedProvidersAsync(
		AppDbContext database, IEnumerable<FeedProvider> providers, ICollection<StoredFeedNode> allNodes)
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
			
			IReadOnlyStoredFeedNode rootNode;
			if (existingRootNode != null)
			{
				rootNode = await LoadFeedNodeAsync(database, existingRootNode, provider, storage, allNodes);
			}
			else
			{
				var (storedNode, dbNode) =
					await StoreFeedNodeAsync(database, provider.CreateInitialTree(), provider, null, allNodes);
				await database.FeedProviders.AddAsync(
					new FeedProviderDb { Identifier = provider.Metadata.Identifier, RootNode = dbNode });
				rootNode = storedNode;
			}

			result.Add(new LoadedFeedProvider(provider, rootNode, storage));
		}

		await database.SaveChangesAsync();
		
		return result;
	}

	private async Task InitializeAsyncCore()
	{
		var providers = _pluginService.GetAvailableFeedProviders();
		var allNodes = new List<StoredFeedNode>();
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
	private readonly Dictionary<Guid, StoredFeedNode> _feedNodes = new();
	private readonly Dictionary<Guid, StoredItem> _items = new();
}
