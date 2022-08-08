using System;
using FluentFeeds.Feeds.Base;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace FluentFeeds.App.Shared.Models.Nodes;

/// <summary>
/// Base class for an node in the feed tree.
/// </summary>
public abstract class FeedNode : ObservableObject
{
	protected FeedNode(string title, Symbol symbol)
	{
		_title = title;
		_symbol = symbol;
		_feed = new Lazy<Feed>(DoCreateFeed);
	}
	
	/// <summary>
	/// Accept a visitor for this node.
	/// </summary>
	public abstract void Accept(IFeedNodeVisitor visitor);

	/// <summary>
	/// Lazily create the feed for this node.
	/// </summary>
	protected abstract Feed DoCreateFeed();
	
	/// <summary>
	/// The type of this node.
	/// </summary>
	public abstract FeedNodeType Type { get; }

	/// <summary>
	/// The feed object (may be created lazily).
	/// </summary>
	public Feed Feed => _feed.Value;
	
	/// <summary>
	/// The title of this node.
	/// </summary>
	public string Title
	{
		get => _title;
		set => SetProperty(ref _title, value);
	}

	/// <summary>
	/// Symbol representing this node.
	/// </summary>
	public Symbol Symbol
	{
		get => _symbol;
		set => SetProperty(ref _symbol, value);
	}

	private readonly Lazy<Feed> _feed;
	private string _title;
	private Symbol _symbol;
}
