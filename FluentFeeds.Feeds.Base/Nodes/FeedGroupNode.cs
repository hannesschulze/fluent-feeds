using System;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;
using FluentFeeds.Common;

namespace FluentFeeds.Feeds.Base.Nodes;

/// <summary>
/// A node in the tree of feeds which can contain other nodes.
/// </summary>
public sealed class FeedGroupNode : FeedNode, IReadOnlyFeedGroupNode
{
	public FeedGroupNode(
		Guid identifier, string title, Symbol symbol = Symbol.Directory, bool isUserCustomizable = false,
		params IReadOnlyFeedNode[] children) 
		: base(identifier, title, symbol)
	{
		Children = new ObservableCollection<IReadOnlyFeedNode>(children);
		_readOnlyChildren = new ReadOnlyObservableCollection<IReadOnlyFeedNode>(Children);
		_isUserCustomizable = isUserCustomizable;
	}

	/// <summary>
	/// Child nodes of this group.
	/// </summary>
	public ObservableCollection<IReadOnlyFeedNode> Children { get; }

	ReadOnlyObservableCollection<IReadOnlyFeedNode> IReadOnlyFeedGroupNode.Children => _readOnlyChildren;

	public bool IsUserCustomizable
	{
		get => _isUserCustomizable;
		set => SetProperty(ref _isUserCustomizable, value);
	}

	protected override Feed DoCreateFeed()
	{
		var result = new CompositeFeed(Children.Select(node => node.Feed));
		Children.CollectionChanged += (s, e) =>
			result.Feeds = Children.Select(node => node.Feed).ToImmutableHashSet();
		return result;
	}

	public override void Accept(IFeedNodeVisitor visitor) => visitor.Visit(this);

	public override FeedNodeType Type => FeedNodeType.Group;

	private readonly ReadOnlyObservableCollection<IReadOnlyFeedNode> _readOnlyChildren;
	private bool _isUserCustomizable;
}
