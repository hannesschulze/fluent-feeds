using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;
using FluentFeeds.Common;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace FluentFeeds.Feeds.Base.Nodes;

/// <summary>
/// A mutable feed node.
/// </summary>
public class FeedNode : ObservableObject, IReadOnlyFeedNode
{
	protected FeedNode(
		FeedNodeType type, Feed? feed, string? title, Symbol? symbol, bool isUserCustomizable,
		IEnumerable<IReadOnlyFeedNode>? children)
	{
		Type = type;
		_feed = new Lazy<Feed>(() =>
			type switch
			{
				FeedNodeType.Custom when feed != null => feed,
				FeedNodeType.Group when Children != null => CreateGroupFeed(Children),
				_ => throw new IndexOutOfRangeException()
			});
		if (children != null)
		{
			_children = new ObservableCollection<IReadOnlyFeedNode>(children);
			_readOnlyChildren = new ReadOnlyObservableCollection<IReadOnlyFeedNode>(_children);
		}
		_title = title;
		_symbol = symbol;
		_isUserCustomizable = isUserCustomizable;
	}

	/// <summary>
	/// Create a group feed node.
	/// </summary>
	public static FeedNode Group(
		string? title, Symbol? symbol, bool isUserCustomizable, IEnumerable<IReadOnlyFeedNode> children) =>
		new(FeedNodeType.Group, null, title, symbol, isUserCustomizable, children);

	/// <summary>
	/// Create a group feed node.
	/// </summary>
	public static FeedNode Group(
		string? title, Symbol? symbol, bool isUserCustomizable, params IReadOnlyFeedNode[] children) =>
		Group(title, symbol, isUserCustomizable, children.AsEnumerable());

	/// <summary>
	/// Create a custom feed node. The feed provider needs to be able to serialize/deserialize the feed object.
	/// </summary>
	public static FeedNode Custom(
		Feed feed, string? title, Symbol? symbol, bool isUserCustomizable,
		IEnumerable<IReadOnlyFeedNode>? children = null) =>
		new(FeedNodeType.Custom, feed, title, symbol, isUserCustomizable, children);

	public FeedNodeType Type { get; }

	public Feed Feed => _feed.Value;

	public ObservableCollection<IReadOnlyFeedNode>? Children => _children;

	ReadOnlyObservableCollection<IReadOnlyFeedNode>? IReadOnlyFeedNode.Children => _readOnlyChildren;

	public string? Title
	{
		get => _title;
		set => SetProperty(ref _title, value);
	}

	public Symbol? Symbol
	{
		get => _symbol;
		set => SetProperty(ref _symbol, value);
	}

	public bool IsUserCustomizable
	{
		get => _isUserCustomizable;
		set => SetProperty(ref _isUserCustomizable, value);
	}

	private static Feed CreateGroupFeed(ObservableCollection<IReadOnlyFeedNode> children)
	{
		var result = new CompositeFeed(children.Select(node => node.Feed));
		children.CollectionChanged += (s, e) =>
			result.Feeds = children.Select(node => node.Feed).ToImmutableHashSet();
		return result;
	}

	private readonly Lazy<Feed> _feed;
	private readonly ObservableCollection<IReadOnlyFeedNode>? _children;
	private readonly ReadOnlyObservableCollection<IReadOnlyFeedNode>? _readOnlyChildren;
	private string? _title;
	private Symbol? _symbol;
	private bool _isUserCustomizable;
}