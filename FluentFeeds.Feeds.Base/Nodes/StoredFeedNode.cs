using System;
using System.Collections.Generic;
using FluentFeeds.Common;

namespace FluentFeeds.Feeds.Base.Nodes;

/// <summary>
/// Mutable representation of a persistently stored feed node.
/// </summary>
public class StoredFeedNode : FeedNode, IReadOnlyStoredFeedNode
{
	protected StoredFeedNode(
		Guid identifier, FeedNodeType type, Feed? feed, string? title, Symbol? symbol, bool isUserCustomizable,
		IEnumerable<IReadOnlyFeedNode>? children) : base(type, feed, title, symbol, isUserCustomizable, children)
	{
		Identifier = identifier;
	}

	public Guid Identifier { get; }
}
