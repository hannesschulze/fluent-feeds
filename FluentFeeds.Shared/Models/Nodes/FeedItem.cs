using FluentFeeds.Shared.Models.Feeds;

namespace FluentFeeds.Shared.Models.Nodes;

/// <summary>
/// A leaf node in the tree of feeds representing a single feed.
/// </summary>
public sealed class FeedItem : FeedNode
{
	public FeedItem(string title, Symbol symbol, Feed feed) : base(title, symbol)
	{
		_feed = feed;
	}

	public override void Accept(IFeedNodeVisitor visitor) => visitor.Visit(this);

	protected override Feed DoCreateFeed() => _feed;

	public override FeedNodeType Type => FeedNodeType.Item;

	private readonly Feed _feed;
}
