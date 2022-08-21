using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using FluentFeeds.App.Shared.Models;
using FluentFeeds.Feeds.Base;
using FluentFeeds.Feeds.Base.Nodes;

namespace FluentFeeds.App.Shared.Services;

/// <summary>
/// Service managing feed providers and the tree of feed nodes.
/// </summary>
public interface IFeedService
{
	/// <summary>
	/// Load the available feeds asynchronously from the local database.
	/// </summary>
	Task InitializeAsync();

	/// <summary>
	/// Get the local representation of a node with the provided identifier.
	/// </summary>
	IReadOnlyStoredFeedNode? GetNode(Guid identifier);

	/// <summary>
	/// Get the parent node's local representation for a node with the provided identifier.
	/// </summary>
	IReadOnlyStoredFeedNode? GetParentNode(Guid identifier);

	/// <summary>
	/// Add a group node to <c>parentIdentifier</c> with the provided name.
	/// </summary>
	Task AddGroupNodeAsync(Guid parentIdentifier, string name);

	/// <summary>
	/// Add a feed node to <c>parentIdentifier</c>.
	/// </summary>
	Task AddFeedNodeAsync(Guid parentIdentifier, Feed feed);

	/// <summary>
	/// Delete the node with the provided identifier from the tree.
	/// </summary>
	Task DeleteNodeAsync(Guid identifier);

	/// <summary>
	/// Rename and/or move the node with the specified identifier.
	/// </summary>
	Task EditNodeAsync(Guid identifier, Guid parentIdentifier, string name);
	
	/// <summary>
	/// A list of loaded feed providers, initially empty.
	/// </summary>
	ReadOnlyObservableCollection<LoadedFeedProvider> FeedProviders { get; }
	
	/// <summary>
	/// Feed node providing an overview of all available feeds.
	/// </summary>
	IReadOnlyFeedNode OverviewFeed { get; }
}
