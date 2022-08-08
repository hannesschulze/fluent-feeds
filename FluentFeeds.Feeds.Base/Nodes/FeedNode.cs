using System;
using FluentFeeds.Common;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace FluentFeeds.Feeds.Base.Nodes;

/// <summary>
/// Base class for an node in the feed tree.
/// </summary>
public abstract class FeedNode : ObservableObject, IReadOnlyFeedNode
{
	protected FeedNode(Guid identifier, string title, Symbol symbol)
	{
		Identifier = identifier;
		_title = title;
		_symbol = symbol;
		_feed = new Lazy<Feed>(DoCreateFeed, isThreadSafe: false);
	}
	
	public abstract void Accept(IFeedNodeVisitor visitor);

	protected abstract Feed DoCreateFeed();
	
	public abstract FeedNodeType Type { get; }
	
	public Guid Identifier { get; }

	public Feed Feed => _feed.Value;
	
	public string Title
	{
		get => _title;
		set => SetProperty(ref _title, value);
	}

	public Symbol Symbol
	{
		get => _symbol;
		set => SetProperty(ref _symbol, value);
	}

	private readonly Lazy<Feed> _feed;
	private string _title;
	private Symbol _symbol;
}
