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
using FluentFeeds.Feeds.Base.Nodes;
using FluentFeeds.Feeds.Base.Storage;
using Microsoft.EntityFrameworkCore;

namespace FluentFeeds.App.Shared.Services.Default;

public class FeedService : IFeedService
{
	private sealed class FeedStorage : IFeedStorage
	{
		public FeedStorage(FeedService feedService, Guid providerIdentifier)
		{
			_feedService = feedService;
			_providerIdentifier = providerIdentifier;
		}
		
		public IItemStorage GetItemStorage(Guid identifier)
		{
			throw new NotImplementedException();
		}

		private readonly FeedService _feedService;
		private readonly Guid _providerIdentifier;
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
		IEnumerable<IReadOnlyFeedNode>? children = null;
		if (node.HasChildren)
		{
			var dbChildren = await database.FeedNodes.Where(n => n.Parent == node).ToListAsync();
			var childTasks = dbChildren.Select(child => LoadFeedNodeAsync(database, child, provider, storage)); 
			children = await Task.WhenAll(childTasks);
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
	private async Task<IReadOnlyStoredFeedNode> StoreFeedNodeAsync(
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

		IEnumerable<IReadOnlyFeedNode>? children = null;
		if (node.Children != null)
		{
			var childTasks = node.Children.Select(child => StoreFeedNodeAsync(database, child, provider, dbNode));
			children = await Task.WhenAll(childTasks);
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

		if (parent == null)
		{
			// This is the root node for the feed provider.
			database.FeedProviders.Add(
				new FeedProviderDb { Identifier = provider.Metadata.Identifier, RootNode = dbNode });
		}
		return storedNode;
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
			var rootNode = existingRootNode != null
				? await LoadFeedNodeAsync(database, existingRootNode, provider, storage)
				: await StoreFeedNodeAsync(database, provider.CreateInitialTree(), provider, null);

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
	private bool _feedProvidersLoaded;
	private Task? _loadFeedProvidersTask;
}
