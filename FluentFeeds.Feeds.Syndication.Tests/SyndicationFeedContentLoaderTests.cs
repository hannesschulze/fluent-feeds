using System;
using System.ComponentModel;
using System.ServiceModel.Syndication;
using System.Threading.Tasks;
using FluentFeeds.Common;
using FluentFeeds.Feeds.Base;
using FluentFeeds.Feeds.Base.Feeds.Content;
using FluentFeeds.Feeds.Syndication.Tests.Mock;
using Xunit;
using SysSyndicationFeed = System.ServiceModel.Syndication.SyndicationFeed;

namespace FluentFeeds.Feeds.Syndication.Tests;

public class SyndicationFeedContentLoaderTests
{
	public SyndicationFeedContentLoaderTests()
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
		ContentLoader = new SyndicationFeedContentLoader(Downloader, new Uri("https://www.example.com"));
	}
	
	private FeedDownloaderMock Downloader { get; }
	private SyndicationFeedContentLoader ContentLoader { get; }
	
	[Fact]
	public async Task Load()
	{
		var content = await ContentLoader.LoadAsync();
		Assert.Equal(new FeedMetadata { Name = "My blog", Symbol = Symbol.Web }, content.Metadata);
		Assert.Collection(
			content.Items,
			item => Assert.Equal("Test item", item.Title));
	}
}
