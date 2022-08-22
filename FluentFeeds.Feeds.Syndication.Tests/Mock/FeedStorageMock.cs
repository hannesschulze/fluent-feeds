using System;
using System.Threading.Tasks;
using FluentFeeds.Feeds.Base;
using FluentFeeds.Feeds.Base.EventArgs;
using FluentFeeds.Feeds.Base.Nodes;
using FluentFeeds.Feeds.Base.Storage;

namespace FluentFeeds.Feeds.Syndication.Tests.Mock;

public sealed class FeedStorageMock : IFeedStorage
{
	public FeedProvider Provider => throw new NotSupportedException();
	
	public event EventHandler<FeedNodesDeletedEventArgs>? NodesDeleted;

	public IItemStorage GetItemStorage(Guid identifier, IItemContentSerializer? contentSerializer = null) =>
		new ItemStorageMock(identifier);

	public IReadOnlyStoredFeedNode GetNode(Guid identifier) =>
		throw new NotSupportedException();

	public IReadOnlyStoredFeedNode GetNodeParent(Guid identifier) =>
		throw new NotSupportedException();

	public Task<IReadOnlyStoredFeedNode> AddNodeAsync(IReadOnlyFeedNode node, Guid parentIdentifier) =>
		throw new NotSupportedException();

	public Task<IReadOnlyStoredFeedNode> RenameNodeAsync(Guid identifier, string newTitle) =>
		throw new NotSupportedException();

	public Task<IReadOnlyStoredFeedNode> MoveNodeAsync(Guid identifier, Guid newParentIdentifier) =>
		throw new NotSupportedException();

	public Task DeleteNodeAsync(Guid identifier) =>
		throw new NotSupportedException();
}
