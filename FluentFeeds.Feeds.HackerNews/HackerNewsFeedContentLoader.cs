using System.Threading.Tasks;
using FluentFeeds.Feeds.Base.Feeds.Content;
using FluentFeeds.Feeds.HackerNews.Download;

namespace FluentFeeds.Feeds.HackerNews;

/// <summary>
/// Class responsible for loading an overview for a Hacker News feed.
/// </summary>
public sealed class HackerNewsFeedContentLoader : IFeedContentLoader
{
	public HackerNewsFeedContentLoader(IDownloader downloader, HackerNewsFeedType feedType)
	{
		Downloader = downloader;
		FeedType = feedType;
	}
	
	/// <summary>
	/// Object used to download the feed content.
	/// </summary>
	public IDownloader Downloader { get; }
	
	/// <summary>
	/// The feed loaded by this object.
	/// </summary>
	public HackerNewsFeedType FeedType { get; }
	
	public Task<FeedContent> LoadAsync()
	{
		return Task.FromResult(new FeedContent());
	}
}
