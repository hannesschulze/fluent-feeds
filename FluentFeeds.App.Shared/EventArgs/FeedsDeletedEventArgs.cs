using System.Collections.Generic;
using FluentFeeds.App.Shared.Models.Feeds;
using FluentFeeds.App.Shared.Models.Storage;

namespace FluentFeeds.App.Shared.EventArgs;

/// <summary>
/// Event args when feeds were deleted from an <see cref="IFeedStorage"/>.
/// </summary>
public sealed class FeedsDeletedEventArgs : System.EventArgs
{
	public FeedsDeletedEventArgs(IReadOnlyCollection<IFeedView> feeds)
	{
		Feeds = feeds;
	}

	/// <summary>
	/// The deleted feeds.
	/// </summary>
	public IReadOnlyCollection<IFeedView> Feeds { get; }
}
