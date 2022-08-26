using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using FluentFeeds.App.Shared.Models.Database;
using FluentFeeds.App.Shared.Models.Feeds;
using FluentFeeds.App.Shared.Models.Feeds.Loaders;
using FluentFeeds.App.Shared.Models.Storage;
using FluentFeeds.App.Shared.Resources;
using FluentFeeds.Common;
using FluentFeeds.Feeds.Base;
using FluentFeeds.Feeds.Base.Feeds.Content;

namespace FluentFeeds.App.Shared.Services.Default;

public sealed class FeedService : IFeedService
{
	private const int MaxItemContentCacheSize = 8;
	
	public FeedService(IDatabaseService databaseService, IPluginService pluginService)
	{
		_databaseService = databaseService;
		_pluginService = pluginService;
		_itemContentCache = new ItemContentCache(MaxItemContentCacheSize);
		_initialize = new Lazy<Task>(InitializeAsyncCore);

		ProviderFeeds = new ReadOnlyObservableCollection<IFeedView>(_providerFeeds);
		OverviewFeed = new Feed(
			identifier: Guid.Parse("1aa31b77-a8d5-426a-8df5-4d6d331051d2"),
			storage: null,
			loaderFactory: _ => _overviewFeedLoader,
			hasChildren: false,
			parent: null,
			name: LocalizedStrings.BuiltInFeedOverviewName,
			symbol: Symbol.Home,
			metadata: new FeedMetadata(),
			isUserCustomizable: false,
			isExcludedFromGroup: false);
	}
	
	public ReadOnlyObservableCollection<IFeedView> ProviderFeeds { get; }
	
	public IFeedView OverviewFeed { get; }
	
	public Task InitializeAsync() => _initialize.Value;

	private async Task<List<IFeedView>> LoadFeedProvidersAsync(
		AppDbContext database, IReadOnlyCollection<FeedProvider> providers)
	{
		var result = new List<IFeedView> { Capacity = providers.Count };
		foreach (var provider in providers)
		{
			var storage = new FeedStorage(_databaseService, _itemContentCache, provider);
			result.Add(await storage.InitializeAsync(database));
		}
		
		return result;
	}

	private async Task InitializeAsyncCore()
	{
		var providers = _pluginService.GetAvailableFeedProviders().ToList();
		var feeds = await _databaseService.ExecuteAsync(
			async database =>
			{
				var result = await LoadFeedProvidersAsync(database, providers);
				await database.SaveChangesAsync();
				return result;
			});

		foreach (var feed in feeds)
		{
			_providerFeeds.Add(feed);
		}
		_overviewFeedLoader.Loaders = feeds
			.Where(feed => !feed.IsExcludedFromGroup)
			.Select(feed => feed.Loader)
			.ToImmutableHashSet();
	}

	private readonly IDatabaseService _databaseService;
	private readonly IPluginService _pluginService;
	private readonly ItemContentCache _itemContentCache;
	private readonly Lazy<Task> _initialize;
	private readonly ObservableCollection<IFeedView> _providerFeeds = new();
	private readonly GroupFeedLoader _overviewFeedLoader = new(ImmutableHashSet<FeedLoader>.Empty);
}
