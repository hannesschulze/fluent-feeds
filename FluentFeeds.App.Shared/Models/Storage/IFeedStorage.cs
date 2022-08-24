using System;
using System.Threading.Tasks;
using FluentFeeds.App.Shared.EventArgs;
using FluentFeeds.App.Shared.Models.Feeds;
using FluentFeeds.App.Shared.Models.Feeds.Loaders;
using FluentFeeds.Feeds.Base;
using FluentFeeds.Feeds.Base.Feeds;
using FluentFeeds.Feeds.Base.Feeds.Content;

namespace FluentFeeds.App.Shared.Models.Storage;

/// <summary>
/// Storage abstraction for feed objects.
/// </summary>
public interface IFeedStorage
{
	/// <summary>
	/// The provider for this storage.
	/// </summary>
	FeedProvider Provider { get; }

	/// <summary>
	/// Event raised when feeds were permanently deleted from the storage.
	/// </summary>
	event EventHandler<FeedsDeletedEventArgs>? FeedsDeleted;

	/// <summary>
	/// Return the item storage with the specified identifier.
	/// </summary>
	IItemStorage GetItemStorage(Guid identifier);

	/// <summary>
	/// Get the local representation of a node with the provided identifier.
	/// </summary>
	IFeedView? GetFeed(Guid identifier);

	/// <summary>
	/// Add a new feed to the group with <c>parentIdentifier</c>.
	/// </summary>
	/// <param name="syncFirst">
	/// Flag indicating that the feed should be synchronized before being added to the group.
	/// </param>
	Task<IFeedView> AddFeedAsync(FeedDescriptor descriptor, Guid parentIdentifier, bool syncFirst = false);

	/// <summary>
	/// Rename the feed with the specified identifier.
	/// </summary>
	Task<IFeedView> RenameFeedAsync(Guid identifier, string newName);

	/// <summary>
	/// Move the feed with the specified identifier into the feed with <c>newParentIdentifier</c>.
	/// </summary>
	Task<IFeedView> MoveFeedAsync(Guid identifier, Guid newParentIdentifier);

	/// <summary>
	/// Update a feed's metadata property.
	/// </summary>
	Task<IFeedView> UpdateFeedMetadataAsync(Guid identifier, FeedMetadata newMetadata);

	/// <summary>
	/// Delete the feed with the provided identifier.
	/// </summary>
	Task DeleteFeedAsync(Guid identifier);
}
