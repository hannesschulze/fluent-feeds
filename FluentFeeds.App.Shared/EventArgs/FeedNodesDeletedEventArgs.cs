using System.Collections.Generic;
using FluentFeeds.Feeds.Base.Nodes;

namespace FluentFeeds.App.Shared.EventArgs;

/// <summary>
/// Event args when feed nodes were deleted from an <see cref="IFeedStorage"/>.
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
