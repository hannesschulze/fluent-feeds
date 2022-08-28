using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using FluentFeeds.Common;
using FluentFeeds.Feeds.Base.Feeds;
using FluentFeeds.Feeds.Syndication.Download;
using FluentFeeds.Feeds.Syndication.Tests.Mock;
using Xunit;

namespace FluentFeeds.Feeds.Syndication.Tests;

public class SyndicationFeedProviderTests
{
	private SyndicationFeedProvider Provider { get; } = new();

	public SyndicationFeedProviderTests()
	{
		Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
	}
	
	[Fact]
	public void InitialStructure()
	{
		var root = Provider.CreateInitialTree();
		Assert.Equal(Symbol.Feed, root.Symbol);
		Assert.Equal("RSS/Atom feeds", root.Name);
		Assert.True(root.IsUserCustomizable);
		Assert.False(root.IsExcludedFromGroup);
		Assert.Collection(
			root.Children,
			child =>
			{
				var descriptor = Assert.IsType<CachedFeedDescriptor>(child);
				Assert.Equal("Fluent Feeds Blog", descriptor.Name);
				Assert.Equal(Symbol.Web, descriptor.Symbol);
				var feed = Assert.IsType<SyndicationFeedContentLoader>(descriptor.ContentLoader);
				var downloader = Assert.IsType<FeedDownloader>(feed.Downloader);
				Assert.Equal(feed.Url, downloader.Url);
			});
	}

	[Fact]
	public void Factories()
	{
		Assert.NotNull(Provider.UrlFeedFactory);
		var url = new Uri("https://www.example.com/");
		var feed = Assert.IsType<SyndicationFeedContentLoader>(Provider.UrlFeedFactory!.Create(url));
		var newDownloader = Assert.IsType<FeedDownloader>(feed.Downloader);
		Assert.Equal(url, newDownloader.Url);
		Assert.Equal(url, feed.Url);
	}

	[Fact]
	public async Task FeedSerialization()
	{
		var downloader = new FeedDownloaderMock();
		var url = new Uri("https://www.example.com/");
		var feed = new SyndicationFeedContentLoader(downloader, url);
		var serialized = await Provider.StoreFeedAsync(feed);
		var deserialized = Assert.IsType<SyndicationFeedContentLoader>(await Provider.LoadFeedAsync(serialized));
		var newDownloader = Assert.IsType<FeedDownloader>(deserialized.Downloader);
		Assert.Equal(url, newDownloader.Url);
		Assert.Equal(url, deserialized.Url);
	}
}
