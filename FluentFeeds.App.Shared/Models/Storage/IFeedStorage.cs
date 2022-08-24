using System;
using System.Threading.Tasks;
using FluentFeeds.App.Shared.EventArgs;
using FluentFeeds.Feeds.Base;
using FluentFeeds.Feeds.Base.Nodes;

namespace FluentFeeds.App.Shared.Models.Storage;

/// <summary>
/// Storage abstraction for feed node objects.
/// </summary>
public interface IFeedStorage
{
	/// <summary>
	/// The provider managing this storage.
	/// </summary>
	FeedProvider Provider { get; }

	/// <summary>
	/// Event raised when nodes were permanently deleted from the storage.
	/// </summary>
	event EventHandler<FeedNodesDeletedEventArgs>? NodesDeleted;

	/// <summary>
	/// Return the item storage with the specified identifier.
	/// </summary>
	IItemStorage GetItemStorage(Guid identifier);

	/// <summary>
	/// Get the local representation of a node with the provided identifier.
	/// </summary>
	IReadOnlyStoredFeedNode? GetNode(Guid identifier);

	/// <summary>
	/// Get the parent node's local representation for a node with the provided identifier.
	/// </summary>
	IReadOnlyStoredFeedNode? GetNodeParent(Guid identifier);

	/// <summary>
	/// Add a new child node to node with <c>parentIdentifier</c>. 
	/// </summary>
	Task<IReadOnlyStoredFeedNode> AddNodeAsync(IReadOnlyFeedNode node, Guid parentIdentifier);

	/// <summary>
	/// Rename the node with the specified identifier.
	/// </summary>
	Task<IReadOnlyStoredFeedNode> RenameNodeAsync(Guid identifier, string newTitle);

	/// <summary>
	/// Move the node with the specified identifier into the node with <c>newParentIdentifier</c>.
	/// </summary>
	Task<IReadOnlyStoredFeedNode> MoveNodeAsync(Guid identifier, Guid newParentIdentifier);

	/// <summary>
	/// Delete the node with the provided identifier.
	/// </summary>
	Task DeleteNodeAsync(Guid identifier);
}
