using System;
using System.ServiceModel.Syndication;
using System.Threading.Tasks;
using FluentFeeds.Common;
using FluentFeeds.Feeds.Base;
using FluentFeeds.Feeds.Base.Storage;
using FluentFeeds.Feeds.Syndication.Download;
using FluentFeeds.Feeds.Syndication.Tests.Mock;
using Xunit;
using SysSyndicationFeed = System.ServiceModel.Syndication.SyndicationFeed;

namespace FluentFeeds.Feeds.Syndication.Tests;

public class SyndicationFeedUrlFactoryTests
{
	private sealed class CustomFactory : SyndicationUrlFeedFactory
	{
		public CustomFactory(IFeedStorage feedStorage, Uri url, SysSyndicationFeed feed) : base(feedStorage)
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
		var factory = new CustomFactory(new FeedStorageMock(), url, source);
		var feed = Assert.IsType<SyndicationFeed>(await factory.CreateAsync(url));
		Assert.Equal(new FeedMetadata(Name: "My blog", null, null, Symbol.Web), feed.Metadata);
		Assert.Equal(feed.Identifier, feed.CollectionIdentifier);
		var storage = Assert.IsType<ItemStorageMock>(feed.Storage);
		Assert.Equal(feed.Identifier, storage.Identifier);
		Assert.Equal(url, feed.Url);
		Assert.Empty(feed.Items);
	}
}
