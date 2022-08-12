using System;
using System.ServiceModel.Syndication;
using System.Threading.Tasks;
using FluentFeeds.Common;
using FluentFeeds.Feeds.Base;
using FluentFeeds.Feeds.Syndication.Tests.Mock;
using Xunit;
using SysSyndicationFeed = System.ServiceModel.Syndication.SyndicationFeed;

namespace FluentFeeds.Feeds.Syndication.Tests;

public class SyndicationFeedTests
{
	private FeedDownloaderMock Downloader { get; }
	private SyndicationFeed Feed { get; }

	public SyndicationFeedTests()
	{
		Downloader =
			new FeedDownloaderMock
			{
				Feed = new SysSyndicationFeed
				{
					Title = new TextSyndicationContent("My <b>blog</b>", TextSyndicationContentKind.Html),
					Items = new[]
					{
						new SyndicationItem
						{
							Title = new TextSyndicationContent("Test <b>item</b>", TextSyndicationContentKind.Html)
						}
					}
				}
			};
		Feed = new SyndicationFeed(
			Downloader, new ItemStorageMock(Guid.Empty), Guid.Empty, new Uri("https://www.example.com"));
	}
	
	[Fact]
	public async Task FetchContent_NotPrefetched()
	{
		await Feed.SynchronizeAsync();
		Assert.Equal(new FeedMetadata(Name: "My blog", null, null, Symbol.Web), Feed.Metadata);
		Assert.Collection(
			Feed.Items,
			item => Assert.Equal("Test item", item.Title));
	}
	
	[Fact]
	public async Task FetchContent_Prefetched()
	{
		await Feed.LoadMetadataAsync();
		Downloader.Feed = new SysSyndicationFeed();
		Assert.Equal(new FeedMetadata(Name: "My blog", null, null, Symbol.Web), Feed.Metadata);
		await Feed.SynchronizeAsync();
		Assert.Equal(new FeedMetadata(Name: "My blog", null, null, Symbol.Web), Feed.Metadata);
		Assert.Collection(
			Feed.Items,
			item => Assert.Equal("Test item", item.Title));
		await Feed.SynchronizeAsync();
		Assert.Equal(new FeedMetadata(null, null, null, Symbol.Web), Feed.Metadata);
		Assert.Collection(
			Feed.Items,
			item => Assert.Equal("Test item", item.Title));
	}
}
