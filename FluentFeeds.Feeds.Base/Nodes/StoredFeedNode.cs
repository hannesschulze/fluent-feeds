using System;
using System.Collections.Generic;
using System.Linq;
using FluentFeeds.Common;

namespace FluentFeeds.Feeds.Base.Nodes;

/// <summary>
/// Mutable representation of a persistently stored feed node.
/// </summary>
public class StoredFeedNode : FeedNode, IReadOnlyStoredFeedNode
{
	internal StoredFeedNode(
		Guid identifier, FeedNodeType type, Feed? feed, string? title, Symbol? symbol, bool isUserCustomizable,
		IEnumerable<IReadOnlyFeedNode>? children) : base(type, feed, title, symbol, isUserCustomizable, children)
	{
		Identifier = identifier;
	}

	/// <summary>
	/// Create a stored feed node from a base feed node.
	/// </summary>
	public StoredFeedNode(IReadOnlyFeedNode node, Guid identifier) : this(
		identifier, node.Type, node.Type == FeedNodeType.Custom ? node.Feed : null, node.Title, node.Symbol, 
		node.IsUserCustomizable, node.Children)
	{
	}
	
	/// <summary>
	/// Create a stored group feed node.
	/// </summary>
	public static StoredFeedNode Group(
		Guid identifier, string? title, Symbol? symbol, bool isUserCustomizable,
		IEnumerable<IReadOnlyFeedNode> children) =>
		new(identifier, FeedNodeType.Group, null, title, symbol, isUserCustomizable, children);

	/// <summary>
	/// Create a stored group feed node.
	/// </summary>
	public static StoredFeedNode Group(
		Guid identifier, string? title, Symbol? symbol, bool isUserCustomizable, params IReadOnlyFeedNode[] children) =>
		Group(identifier, title, symbol, isUserCustomizable, children.AsEnumerable());

	/// <summary>
	/// Create a stored custom feed node. The feed provider needs to be able to serialize/deserialize the feed object.
	/// </summary>
	public static StoredFeedNode Custom(
		Guid identifier, Feed feed, string? title, Symbol? symbol, bool isUserCustomizable,
		IEnumerable<IReadOnlyFeedNode>? children = null) =>
		new(identifier, FeedNodeType.Custom, feed, title, symbol, isUserCustomizable, children);

	public Guid Identifier { get; }
}
