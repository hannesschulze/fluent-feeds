using System;
using System.ComponentModel;
using FluentFeeds.Common;

namespace FluentFeeds.Feeds.Base.Nodes;

/// <summary>
/// Read-only view into a node in the tree of feeds.
/// </summary>
public interface IReadOnlyFeedNode : INotifyPropertyChanged, INotifyPropertyChanging
{
	/// <summary>
	/// Accept a visitor for this node.
	/// </summary>
	void Accept(IFeedNodeVisitor visitor);

	/// <summary>
	/// The type of this node.
	/// </summary>
	FeedNodeType Type { get; }
	
	/// <summary>
	/// Unique identifier for this node.
	/// </summary>
	Guid Identifier { get; }

	/// <summary>
	/// The feed object.
	/// </summary>
	Feed Feed { get; }
	
	/// <summary>
	/// The title of this node.
	/// </summary>
	string Title { get; }

	/// <summary>
	/// Symbol representing this node.
	/// </summary>
	Symbol Symbol { get; }
}
