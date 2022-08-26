using System.Threading.Tasks;
using FluentFeeds.Feeds.Base.Feeds.Content;

namespace FluentFeeds.Feeds.HackerNews;

/// <summary>
/// Class responsible for loading an overview for a Hacker News feed.
/// </summary>
public sealed class HackerNewsFeedContentLoader : IFeedContentLoader
{
	public Task<FeedContent> LoadAsync()
	{
		throw new System.NotImplementedException();
	}
}
