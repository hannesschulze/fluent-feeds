using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using FluentFeeds.App.Shared.Models.Feeds.Loaders;
using FluentFeeds.App.Shared.Services.Default;
using FluentFeeds.App.Shared.Tests.Mock;
using Xunit;
using Xunit.Abstractions;

namespace FluentFeeds.App.Shared.Tests.Services;

public class FeedServiceTests
{
	public FeedServiceTests(ITestOutputHelper testOutputHelper)
	{
		Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
		DatabaseService = new DatabaseServiceMock(testOutputHelper);
		SettingsService = new SettingsServiceMock();
	}
	
	private DatabaseServiceMock DatabaseService { get; }
	private SettingsServiceMock SettingsService { get; }

	[Fact]
	public async Task Initialization_HackerNewsEnabled()
	{
		var service = new FeedService(DatabaseService, SettingsService);
		SettingsService.IsHackerNewsEnabled = true;
		await service.InitializeAsync();
		
		Assert.Collection(
			service.ProviderFeeds,
			feed => Assert.Equal("RSS/Atom feeds", feed.Name),
			feed => Assert.Equal("Hacker News", feed.Name));
		var overviewLoader = Assert.IsType<GroupFeedLoader>(service.OverviewFeed.Loader);
		Assert.Equal(2, overviewLoader.Loaders.Count);
		Assert.Contains(service.ProviderFeeds[0].Loader, overviewLoader.Loaders);
		Assert.Contains(service.ProviderFeeds[1].Loader, overviewLoader.Loaders);
	}

	[Fact]
	public async Task Initialization_HackerNewsDisabled()
	{
		var service = new FeedService(DatabaseService, SettingsService);
		SettingsService.IsHackerNewsEnabled = false;
		await service.InitializeAsync();
		
		Assert.Collection(
			service.ProviderFeeds,
			feed => Assert.Equal("RSS/Atom feeds", feed.Name));
		var overviewLoader = Assert.IsType<GroupFeedLoader>(service.OverviewFeed.Loader);
		Assert.Single(overviewLoader.Loaders);
		Assert.Contains(service.ProviderFeeds[0].Loader, overviewLoader.Loaders);
	}

	[Fact]
	public async Task HackerNewsSetting_Enable()
	{
		SettingsService.IsHackerNewsEnabled = false;
		var service = new FeedService(DatabaseService, SettingsService);
		await service.InitializeAsync();

		SettingsService.IsHackerNewsEnabled = true;
		Assert.Collection(
			service.ProviderFeeds,
			feed => Assert.Equal("RSS/Atom feeds", feed.Name),
			feed => Assert.Equal("Hacker News", feed.Name));
		var overviewLoader = Assert.IsType<GroupFeedLoader>(service.OverviewFeed.Loader);
		Assert.Equal(2, overviewLoader.Loaders.Count);
		Assert.Contains(service.ProviderFeeds[0].Loader, overviewLoader.Loaders);
		Assert.Contains(service.ProviderFeeds[1].Loader, overviewLoader.Loaders);
	}

	[Fact]
	public async Task HackerNewsSetting_Disable()
	{
		SettingsService.IsHackerNewsEnabled = true;
		var service = new FeedService(DatabaseService, SettingsService);
		await service.InitializeAsync();

		SettingsService.IsHackerNewsEnabled = false;
		Assert.Collection(
			service.ProviderFeeds,
			feed => Assert.Equal("RSS/Atom feeds", feed.Name));
		var overviewLoader = Assert.IsType<GroupFeedLoader>(service.OverviewFeed.Loader);
		Assert.Single(overviewLoader.Loaders);
		Assert.Contains(service.ProviderFeeds[0].Loader, overviewLoader.Loaders);
	}
}
