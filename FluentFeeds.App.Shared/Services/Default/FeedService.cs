using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using FluentFeeds.App.Shared.Models.Database;
using FluentFeeds.App.Shared.Models.Feeds.Loaders;
using FluentFeeds.Common;
using FluentFeeds.Feeds.Base;
using FluentFeeds.Feeds.Base.Nodes;

namespace FluentFeeds.App.Shared.Services.Default;

public partial class FeedService : IFeedService
{
	public FeedService(IDatabaseService databaseService, IPluginService pluginService)
	{
		_databaseService = databaseService;
		_pluginService = pluginService;
		_initialize = new Lazy<Task>(InitializeAsyncCore);

		ProviderNodes = new ReadOnlyObservableCollection<IReadOnlyStoredFeedNode>(_providerNodes);
		OverviewNode = FeedNode.Custom(_overviewFeed, "Overview", Symbol.Home, isUserCustomizable: false);
	}
	
	public ReadOnlyObservableCollection<IReadOnlyStoredFeedNode> ProviderNodes { get; }
	
	public IReadOnlyFeedNode OverviewNode { get; }
	
	public Task InitializeAsync() => _initialize.Value;

	private async Task<List<IReadOnlyStoredFeedNode>> LoadFeedProvidersAsync(
		AppDbContext database, IReadOnlyCollection<FeedProvider> providers)
	{
		var result = new List<IReadOnlyStoredFeedNode> { Capacity = providers.Count };
		foreach (var provider in providers)
		{
			var storage = new FeedStorage(_databaseService, provider);
			result.Add(await storage.InitializeAsync(database));
		}
		
		return result;
	}

	private async Task InitializeAsyncCore()
	{
		var providers = _pluginService.GetAvailableFeedProviders().ToList();
		var nodes = await _databaseService.ExecuteAsync(
			async database =>
			{
				var result = await LoadFeedProvidersAsync(database, providers);
				await database.SaveChangesAsync();
				return result;
			});

		foreach (var node in nodes)
		{
			_providerNodes.Add(node);
		}
		_overviewFeed.Feeds = nodes.Select(node => node.Feed).ToImmutableHashSet();
	}

	private readonly IDatabaseService _databaseService;
	private readonly IPluginService _pluginService;
	private readonly Lazy<Task> _initialize;
	private readonly ObservableCollection<IReadOnlyStoredFeedNode> _providerNodes = new();
	private readonly CompositeFeed _overviewFeed = new();
}
