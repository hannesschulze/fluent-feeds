using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentFeeds.App.Shared.EventArgs;
using FluentFeeds.App.Shared.Models.Storage;
using FluentFeeds.Feeds.Base;
using FluentFeeds.Feeds.Base.Nodes;

namespace FluentFeeds.App.Shared.Tests.Mock;

public sealed class FeedStorageMock : IFeedStorage
{
	private sealed class Node : StoredFeedNode
	{
		public Node(IReadOnlyFeedNode node, IFeedStorage storage, Node? parent) : base(node, Guid.NewGuid(), storage)
		{
			Parent = parent;
		}
		
		public Node? Parent { get; set; }
	}
	
	public FeedStorageMock(FeedProvider provider)
	{
		Provider = provider;
	}
	
	public FeedProvider Provider { get; }
	
	public bool DeleteNodeFails { get; set; }
	
	public event EventHandler<FeedNodesDeletedEventArgs>? NodesDeleted;

	public IItemStorage GetItemStorage(Guid identifier)
	{
		return new ItemStorageMock();
	}

	public IReadOnlyStoredFeedNode? GetNode(Guid identifier)
	{
		return _nodes.GetValueOrDefault(identifier);
	}

	public IReadOnlyStoredFeedNode? GetNodeParent(Guid identifier)
	{
		return _nodes.GetValueOrDefault(identifier)?.Parent;
	}

	public IReadOnlyStoredFeedNode AddRootNode(IReadOnlyFeedNode node)
	{
		var stored = new Node(node, this, null);
		_nodes.Add(stored.Identifier, stored);
		return stored;
	}

	public Task<IReadOnlyStoredFeedNode> AddNodeAsync(IReadOnlyFeedNode node, Guid parentIdentifier)
	{
		var parent = _nodes[parentIdentifier];
		var stored = new Node(node, this, parent);
		_nodes.Add(stored.Identifier, stored);
		parent.Children!.Add(stored);
		return Task.FromResult<IReadOnlyStoredFeedNode>(stored);
	}

	public Task<IReadOnlyStoredFeedNode> RenameNodeAsync(Guid identifier, string newTitle)
	{
		var node = _nodes[identifier];
		node.Title = newTitle;
		return Task.FromResult<IReadOnlyStoredFeedNode>(node);
	}

	public Task<IReadOnlyStoredFeedNode> MoveNodeAsync(Guid identifier, Guid newParentIdentifier)
	{
		var node = _nodes[identifier];
		var newParent = _nodes[newParentIdentifier];
		node.Parent?.Children!.Remove(node);
		node.Parent = newParent;
		newParent.Children!.Add(node);
		return Task.FromResult<IReadOnlyStoredFeedNode>(node);
	}

	public Task DeleteNodeAsync(Guid identifier)
	{
		if (DeleteNodeFails)
			throw new Exception("error");
		
		var node = _nodes[identifier];
		node.Parent?.Children!.Remove(node);
		NodesDeleted?.Invoke(this, new FeedNodesDeletedEventArgs(new[] { node }));
		_nodes.Remove(identifier);
		return Task.CompletedTask;
	}

	private readonly Dictionary<Guid, Node> _nodes = new();
}
