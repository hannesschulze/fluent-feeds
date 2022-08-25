using System;
using System.Collections.Immutable;
using System.Threading.Tasks;
using FluentFeeds.App.Shared.Models.Feeds;
using FluentFeeds.App.Shared.Models.Feeds.Loaders;
using FluentFeeds.App.Shared.Services.Default;
using FluentFeeds.App.Shared.Tests.Mock;
using FluentFeeds.Common;
using FluentFeeds.Feeds.Base;
using FluentFeeds.Feeds.Base.Feeds;
using Xunit;
using Xunit.Abstractions;

namespace FluentFeeds.App.Shared.Tests.Services;

public class FeedServiceTests
{
	public FeedServiceTests(ITestOutputHelper testOutputHelper)
	{
		DatabaseService = new DatabaseServiceMock(testOutputHelper);
		PluginService = new PluginServiceMock();
	}
	
	private DatabaseServiceMock DatabaseService { get; }
	private PluginServiceMock PluginService { get; }

	[Fact]
	public async Task Initialization_StoreFeedsForProviders()
	{
		var providerIdentifier = Guid.Parse("6ebe7ccf-e1ae-4930-9c6e-97cf61fec7e3");
		var contentLoaderA = new FeedContentLoaderMock("feed");
		var provider =
			new FeedProviderMock(providerIdentifier)
			{
				InitialTree =
					new GroupFeedDescriptor("root", Symbol.Feed,
						new GroupFeedDescriptor("child", Symbol.Directory,
							new CachedFeedDescriptor(contentLoaderA)
							{
								Name = "feed",
								Symbol = Symbol.Web
							})) { IsUserCustomizable = false }
			};
		PluginService.AvailableFeedProviders = ImmutableArray.Create<FeedProvider>(provider);
		var serviceA = new FeedService(DatabaseService, PluginService);
		await serviceA.InitializeAsync();

		var rootNodeA = Assert.Single(serviceA.ProviderFeeds);
		Assert.NotNull(rootNodeA.Storage);
		Assert.Equal(provider, rootNodeA.Storage!.Provider);
		Assert.Equal(rootNodeA, rootNodeA.Storage!.GetFeed(rootNodeA.Identifier));
		Assert.Null(rootNodeA.Parent);
		Assert.Equal("root", rootNodeA.Name);
		Assert.Equal(Symbol.Feed, rootNodeA.Symbol);
		Assert.False(rootNodeA.IsUserCustomizable);
		Assert.NotNull(rootNodeA.Children);
		var childNodeA = Assert.IsAssignableFrom<IFeedView>(Assert.Single(rootNodeA.Children!));
		Assert.NotNull(childNodeA.Storage);
		Assert.Equal(childNodeA, childNodeA.Storage!.GetFeed(childNodeA.Identifier));
		Assert.Equal(rootNodeA, childNodeA.Parent);
		Assert.Equal("child", childNodeA.Name);
		Assert.Equal(Symbol.Directory, childNodeA.Symbol);
		Assert.True(childNodeA.IsUserCustomizable);
		Assert.NotNull(childNodeA.Children);
		var feedNodeA = Assert.IsAssignableFrom<IFeedView>(Assert.Single(childNodeA.Children!));
		Assert.NotNull(feedNodeA.Storage);
		Assert.Equal(feedNodeA, feedNodeA.Storage!.GetFeed(feedNodeA.Identifier));
		Assert.Equal("feed", feedNodeA.Name);
		Assert.Equal(Symbol.Web, feedNodeA.Symbol);
		Assert.True(feedNodeA.IsUserCustomizable);
		Assert.Null(feedNodeA.Children);
		var feedA = Assert.IsType<CachedFeedLoader>(feedNodeA.Loader);
		Assert.Equal(contentLoaderA, feedA.ContentLoader);
		var overviewChildA =
			Assert.Single(Assert.IsAssignableFrom<GroupFeedLoader>(serviceA.OverviewFeed.Loader).Loaders);
		Assert.Equal(rootNodeA.Loader, overviewChildA);

		provider.InitialTree = new GroupFeedDescriptor();
		var serviceB = new FeedService(DatabaseService, PluginService);
		await serviceB.InitializeAsync();

		var rootNodeB = Assert.Single(serviceB.ProviderFeeds);
		Assert.NotNull(rootNodeB.Storage);
		Assert.Equal(rootNodeB, rootNodeB.Storage!.GetFeed(rootNodeB.Identifier));
		Assert.Equal(rootNodeA.Identifier, rootNodeB.Identifier);
		Assert.Null(rootNodeB.Parent);
		Assert.Equal(rootNodeA.Identifier, rootNodeB.Identifier);
		Assert.Equal("root", rootNodeB.Name);
		Assert.Equal(Symbol.Feed, rootNodeB.Symbol);
		Assert.False(rootNodeB.IsUserCustomizable);
		Assert.NotNull(rootNodeB.Children);
		var childNodeB = Assert.IsAssignableFrom<IFeedView>(Assert.Single(rootNodeB.Children!));
		Assert.NotNull(childNodeB.Storage);
		Assert.Equal(childNodeB, childNodeB.Storage!.GetFeed(childNodeB.Identifier));
		Assert.Equal(childNodeA.Identifier, childNodeB.Identifier);
		Assert.Equal(rootNodeB, childNodeB.Parent);
		Assert.Equal("child", childNodeB.Name);
		Assert.Equal(Symbol.Directory, childNodeB.Symbol);
		Assert.True(childNodeB.IsUserCustomizable);
		Assert.NotNull(childNodeB.Children);
		var feedNodeB = Assert.IsAssignableFrom<IFeedView>(Assert.Single(childNodeB.Children!));
		Assert.NotNull(feedNodeB.Storage);
		Assert.Equal(feedNodeB, feedNodeB.Storage!.GetFeed(feedNodeB.Identifier));
		Assert.Equal(feedNodeA.Identifier, feedNodeB.Identifier);
		Assert.Equal(childNodeB, feedNodeB.Parent);
		Assert.Equal("feed", feedNodeB.Name);
		Assert.Equal(Symbol.Web, feedNodeB.Symbol);
		Assert.True(feedNodeB.IsUserCustomizable);
		Assert.Null(feedNodeB.Children);
		var contentLoaderB = Assert.IsType<FeedContentLoaderMock>(
			Assert.IsType<CachedFeedLoader>(feedNodeB.Loader).ContentLoader);
		Assert.Equal("feed", contentLoaderB.Identifier);
		var overviewChildB =
			Assert.Single(Assert.IsAssignableFrom<GroupFeedLoader>(serviceB.OverviewFeed.Loader).Loaders);
		Assert.Equal(rootNodeB.Loader, overviewChildB);
	}
}
