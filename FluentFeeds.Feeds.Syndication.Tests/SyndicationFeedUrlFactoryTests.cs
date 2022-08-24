using System;
using System.ServiceModel.Syndication;
using System.Threading.Tasks;
using FluentFeeds.Common;
using FluentFeeds.Feeds.Base;
using FluentFeeds.Feeds.Base.Feeds.Content;
using FluentFeeds.Feeds.Syndication.Download;
using FluentFeeds.Feeds.Syndication.Tests.Mock;
using Xunit;
using SysSyndicationFeed = System.ServiceModel.Syndication.SyndicationFeed;

namespace FluentFeeds.Feeds.Syndication.Tests;

public class SyndicationFeedUrlFactoryTests
{
	private sealed class CustomFactory : SyndicationUrlFeedFactory
	{
		public CustomFactory(Uri url, SysSyndicationFeed feed)
		{
			_url = url;
			_feed = feed;
		}

		protected override IFeedDownloader CreateDownloader(Uri url)
		{
			Assert.Equal(_url, url);
			return new FeedDownloaderMock { Feed = _feed };
		}

		private readonly Uri _url;
		private readonly SysSyndicationFeed _feed;
	}
	
	[Fact]
	public async Task CreateFeed()
	{
		var source = 
			new SysSyndicationFeed
			{
				Title = new TextSyndicationContent("My <b>blog</b>", TextSyndicationContentKind.Html),
				Items = new[] { new SyndicationItem() }
			};
		var url = new Uri("https://www.example.com");
		var factory = new CustomFactory(url, source);
		var feed = Assert.IsType<SyndicationFeed>(await factory.CreateAsync(new FeedStorageMock(), url));
		Assert.Equal(new FeedMetadata { Name = "My blog", Symbol = Symbol.Web }, feed.Metadata);
		var storage = Assert.IsType<ItemStorageMock>(feed.Storage);
		Assert.Equal(feed.Identifier, storage.Identifier);
		Assert.Equal(url, feed.Url);
		Assert.Empty(feed.Items);
	}
}
