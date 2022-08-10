using FluentFeeds.Common;
using FluentFeeds.Feeds.Base;
using FluentFeeds.Feeds.Base.Nodes;
using FluentFeeds.Feeds.Base.Storage;

namespace FluentFeeds.Feeds.Syndication;

public sealed class SyndicationFeedProvider : FeedProvider
{
	public SyndicationFeedProvider(IFeedStorage storage) : base(storage)
	{
	}

	public override IReadOnlyFeedNode CreateInitialTree()
	{
		return FeedNode.Group(title: "RSS/Atom feeds", symbol: Symbol.Feed, isUserCustomizable: true);
	}

	public override Feed LoadFeed(string serialized)
	{
		throw new System.NotImplementedException();
	}

	public override string StoreFeed(Feed feed)
	{
		throw new System.NotImplementedException();
	}
}
