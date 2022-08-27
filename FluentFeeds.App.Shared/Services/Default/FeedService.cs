using System;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
using FluentFeeds.Feeds.HackerNews;
using FluentFeeds.Feeds.Syndication;

namespace FluentFeeds.App.Shared.Services.Default;

public sealed class FeedService : IFeedService
{
	private const int MaxItemContentCacheSize = 8;
	
	public FeedService(IDatabaseService databaseService, ISettingsService settingsService)
	{
		_databaseService = databaseService;
		_settingsService = settingsService;
		_settingsService.PropertyChanged += HandleSettingsChanged;
		_isHackerNewsFeedVisible = _settingsService.IsHackerNewsEnabled;
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

	private Task<IFeedView> InitializeFeedProviderAsync(AppDbContext database, FeedProvider provider)
	{
		var storage = new FeedStorage(_databaseService, _itemContentCache, provider);
		return storage.InitializeAsync(database);
	}

	private async Task InitializeAsyncCore()
	{
		await _databaseService.ExecuteAsync(
			async database =>
			{
				_syndicationFeed = await InitializeFeedProviderAsync(database, new SyndicationFeedProvider());
				_hackerNewsFeed = await InitializeFeedProviderAsync(database, new HackerNewsFeedProvider());
				await database.SaveChangesAsync();
			});

		_providerFeeds.Add(_syndicationFeed!);
		if (IsHackerNewsFeedVisible)
		{
			_providerFeeds.Add(_hackerNewsFeed!);
		}

		UpdateOverviewFeed();
		_isInitialized = true;
	}

	private bool IsHackerNewsFeedVisible
	{
		get => _isHackerNewsFeedVisible;
		set
		{
			var oldValue = _isHackerNewsFeedVisible;
			_isHackerNewsFeedVisible = value;
			if (value != oldValue && _isInitialized)
			{
				if (value)
				{
					_providerFeeds.Add(_hackerNewsFeed!);
				}
				else
				{
					_providerFeeds.Remove(_hackerNewsFeed!);
				}

				UpdateOverviewFeed();
			}
		}
	}

	private void HandleSettingsChanged(object? sender, PropertyChangedEventArgs e)
	{
		switch (e.PropertyName)
		{
			case nameof(ISettingsService.IsHackerNewsEnabled):
				IsHackerNewsFeedVisible = _settingsService.IsHackerNewsEnabled;
				break;
		}
	}

	private void UpdateOverviewFeed()
	{
		_overviewFeedLoader.Loaders = _providerFeeds
			.Where(feed => !feed.IsExcludedFromGroup)
			.Select(feed => feed.Loader)
			.ToImmutableHashSet();
	}

	private readonly IDatabaseService _databaseService;
	private readonly ISettingsService _settingsService;
	private readonly ItemContentCache _itemContentCache;
	private readonly Lazy<Task> _initialize;
	private readonly ObservableCollection<IFeedView> _providerFeeds = new();
	private readonly GroupFeedLoader _overviewFeedLoader = new(ImmutableHashSet<FeedLoader>.Empty);
	private bool _isHackerNewsFeedVisible;
	private IFeedView? _syndicationFeed;
	private IFeedView? _hackerNewsFeed;
	private bool _isInitialized;
}
