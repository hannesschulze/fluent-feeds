using FluentFeeds.Feeds.Base;
using FluentFeeds.Feeds.Base.Feeds.Content;

namespace FluentFeeds.App.Shared.EventArgs;

/// <summary>
/// Event args when the metadata of a <see cref="Feed"/> was updated.
/// </summary>
public sealed class FeedMetadataUpdatedEventArgs : System.EventArgs
{
	public FeedMetadataUpdatedEventArgs(FeedMetadata updatedMetadata)
	{
		UpdatedMetadata = updatedMetadata;
	}

	/// <summary>
	/// The updated metadata.
	/// </summary>
	public FeedMetadata UpdatedMetadata { get; }
}
