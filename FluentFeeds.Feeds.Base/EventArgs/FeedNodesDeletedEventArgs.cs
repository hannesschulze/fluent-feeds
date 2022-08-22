using System.Collections.Generic;
using FluentFeeds.Feeds.Base.Nodes;
using FluentFeeds.Feeds.Base.Storage;

namespace FluentFeeds.Feeds.Base.EventArgs;

/// <summary>
/// Event args when feed nodes were deleted from a <see cref="IFeedStorage"/>.
/// </summary>
public sealed class FeedNodesDeletedEventArgs : System.EventArgs
{
	public FeedNodesDeletedEventArgs(IReadOnlyCollection<IReadOnlyStoredFeedNode> nodes)
	{
		Nodes = nodes;
	}

	/// <summary>
	/// The deleted nodes.
	/// </summary>
	public IReadOnlyCollection<IReadOnlyStoredFeedNode> Nodes { get; }
}
