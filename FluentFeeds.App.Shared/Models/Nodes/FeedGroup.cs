using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;
using FluentFeeds.Feeds.Base;

namespace FluentFeeds.App.Shared.Models.Nodes;

/// <summary>
/// A node in the tree of feeds which can contain other nodes.
/// </summary>
public sealed class FeedGroup : FeedNode
{
	public FeedGroup(
		string title, Symbol symbol = Symbol.Directory, bool isUserCustomizable = false, params FeedNode[] children) 
		: base(title, symbol)
	{
		_isUserCustomizable = isUserCustomizable;
		Children = new ObservableCollection<FeedNode>(children);
	}

	/// <summary>
	/// Child nodes of this group.
	/// </summary>
	public ObservableCollection<FeedNode> Children { get; }

	/// <summary>
	/// Flag indicating if the user can customize this group (i.e. rename it, drag nodes from it, drop nodes onto it). 
	/// </summary>
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

	private bool _isUserCustomizable;
}
