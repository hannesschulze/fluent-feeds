using System;

namespace FluentFeeds.Feeds.Base.Nodes;

/// <summary>
/// Read-only view into a persistently stored feed node.
/// </summary>
public interface IReadOnlyStoredFeedNode : IReadOnlyFeedNode
{
	/// <summary>
	/// Unique identifier for this node.
	/// </summary>
	Guid Identifier { get; }
}
