using System;
using System.Collections.Generic;
using System.Linq;
using FluentFeeds.Common;
using FluentFeeds.Feeds.Base.Storage;

namespace FluentFeeds.Feeds.Base.Nodes;

/// <summary>
/// Mutable representation of a persistently stored feed node.
/// </summary>
public class StoredFeedNode : FeedNode, IReadOnlyStoredFeedNode
{
	/// <summary>
	/// Create a stored feed node from a base feed node.
	/// </summary>
	public StoredFeedNode(IReadOnlyFeedNode node, Guid identifier, IFeedStorage storage) : base(node)
	{
		Identifier = identifier;
		Storage = storage;
	}

	/// <summary>
	/// Create a copy of another feed node.
	/// </summary>
	public StoredFeedNode(IReadOnlyStoredFeedNode node) : this(node, node.Identifier, node.Storage)
	{
	}
	
	public Guid Identifier { get; }
	
	public IFeedStorage Storage { get; }
}
