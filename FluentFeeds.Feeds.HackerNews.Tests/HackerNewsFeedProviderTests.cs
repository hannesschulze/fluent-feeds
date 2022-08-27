using System.Threading.Tasks;
using FluentFeeds.Common;
using FluentFeeds.Feeds.HackerNews.Download;
using FluentFeeds.Feeds.HackerNews.Tests.Mock;
using Xunit;

namespace FluentFeeds.Feeds.HackerNews.Tests;

public class HackerNewsFeedProviderTests
{
	private HackerNewsFeedProvider Provider { get; } = new();
	
	[Fact]
	public void InitialStructure()
	{
		var root = Provider.CreateInitialTree();
		Assert.Equal(Symbol.HackerNews, root.Symbol);
		Assert.Equal("Hacker News", root.Name);
		Assert.False(root.IsUserCustomizable);
		Assert.False(root.IsExcludedFromGroup);
		Assert.Collection(
			root.Children,
			feed =>
			{
				Assert.Equal(Symbol.Trending, feed.Symbol);
				Assert.Equal("Top", feed.Name);
				Assert.False(feed.IsUserCustomizable);
				Assert.False(feed.IsExcludedFromGroup);
			},
			feed =>
			{
				Assert.Equal(Symbol.Question, feed.Symbol);
				Assert.Equal("Ask", feed.Name);
				Assert.False(feed.IsUserCustomizable);
				Assert.True(feed.IsExcludedFromGroup);
			},
			feed =>
			{
				Assert.Equal(Symbol.Presentation, feed.Symbol);
				Assert.Equal("Show", feed.Name);
				Assert.False(feed.IsUserCustomizable);
				Assert.True(feed.IsExcludedFromGroup);
			},
			feed =>
			{
				Assert.Equal(Symbol.Briefcase, feed.Symbol);
				Assert.Equal("Jobs", feed.Name);
				Assert.False(feed.IsUserCustomizable);
				Assert.True(feed.IsExcludedFromGroup);
			});
	}

	[Fact]
	public async Task FeedSerialization()
	{
		var downloader = new DownloaderMock();
		var loader = new HackerNewsFeedContentLoader(downloader, HackerNewsFeedType.Show);
		var serialized = await Provider.StoreFeedAsync(loader);
		var deserialized = Assert.IsType<HackerNewsFeedContentLoader>(await Provider.LoadFeedAsync(serialized));
		Assert.IsType<Downloader>(deserialized.Downloader);
		Assert.Equal(HackerNewsFeedType.Show, deserialized.FeedType);
	}

	[Fact]
	public async Task ItemSerialization()
	{
		var downloader = new DownloaderMock();
		var loader = new HackerNewsItemContentLoader(downloader, 8863);
		var serialized = await Provider.StoreItemContentAsync(loader);
		var deserialized = Assert.IsType<HackerNewsItemContentLoader>(await Provider.LoadItemContentAsync(serialized));
		Assert.IsType<Downloader>(deserialized.Downloader);
		Assert.Equal(8863, deserialized.Identifier);
	}
}
