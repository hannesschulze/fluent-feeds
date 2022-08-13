using System;
using System.Collections.Immutable;
using System.Threading.Tasks;
using FluentFeeds.App.Shared.Services.Default;
using FluentFeeds.App.Shared.Tests.Services.Mock;
using FluentFeeds.Feeds.Base;
using FluentFeeds.Feeds.Base.Nodes;
using FluentFeeds.Feeds.Syndication;
using Xunit;
using Xunit.Abstractions;

namespace FluentFeeds.App.Shared.Tests.Services;

public class FeedServiceTests
{
	private DatabaseServiceMock DatabaseService { get; }
	private PluginServiceMock PluginService { get; }
	
	public FeedServiceTests(ITestOutputHelper testOutputHelper)
	{
		DatabaseService = new DatabaseServiceMock(testOutputHelper);
		PluginService = new PluginServiceMock();
	}

	[Fact]
	public async Task Test()
	{
		PluginService.AvailableFeedProviders = ImmutableArray.Create<FeedProvider>(new SyndicationFeedProvider());
		var serviceA = new FeedService(DatabaseService, PluginService);
		await serviceA.LoadFeedProvidersAsync();
		var serviceB = new FeedService(DatabaseService, PluginService);
		await serviceB.LoadFeedProvidersAsync();
		Assert.Equal(serviceA.FeedProviders[0].RootNode.Identifier, serviceB.FeedProviders[0].RootNode.Identifier);
	}
}
