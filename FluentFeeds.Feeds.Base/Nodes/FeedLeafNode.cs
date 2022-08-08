using System;
using FluentFeeds.Common;

namespace FluentFeeds.Feeds.Base.Nodes;

/// <summary>
/// A leaf node in the tree of feeds representing a single feed.
/// </summary>
public sealed class FeedLeafNode : FeedNode, IReadOnlyFeedLeafNode
{
	public FeedLeafNode(Guid identifier, string title, Symbol symbol, Feed feed) : base(identifier, title, symbol)
	{
		_feed = feed;
	}

	public override void Accept(IFeedNodeVisitor visitor) => visitor.Visit(this);

	protected override Feed DoCreateFeed() => _feed;

	public override FeedNodeType Type => FeedNodeType.Leaf;

	private readonly Feed _feed;
}
