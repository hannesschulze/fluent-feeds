using System;
using System.Threading.Tasks;
using FluentFeeds.Common;
using FluentFeeds.Feeds.Base;
using FluentFeeds.Feeds.Base.Feeds;
using FluentFeeds.Feeds.Base.Feeds.Content;

namespace FluentFeeds.Feeds.HackerNews;

/// <summary>
/// Feed provider implementation fetching feeds using the public Hacker News API.
/// </summary>
public class HackerNewsFeedProvider : FeedProvider
{
	public HackerNewsFeedProvider()
		: base(new FeedProviderMetadata(
			Identifier: Guid.Parse("9a9d8c17-c940-4069-999a-0016bfdddb11"),
			Name: "Hacker News Feed Provider",
			Description: "A feed provider fetching feeds using the public Hacker News API."))
	{
	}

	public override GroupFeedDescriptor CreateInitialTree()
	{
		return new GroupFeedDescriptor(name: "Hacker News", symbol: Symbol.HackerNews);
	}

	public override Task<string> StoreFeedAsync(IFeedContentLoader contentLoader)
	{
		throw new NotImplementedException();
	}

	public override Task<IFeedContentLoader> LoadFeedAsync(string serialized)
	{
		throw new NotImplementedException();
	}
}
