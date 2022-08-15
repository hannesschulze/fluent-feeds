using System.Collections.ObjectModel;
using System.ComponentModel;
using FluentFeeds.Common;

namespace FluentFeeds.Feeds.Base.Nodes;

/// <summary>
/// Read-only view into a node in the tree of feeds.
/// </summary>
public interface IReadOnlyFeedNode : INotifyPropertyChanged, INotifyPropertyChanging
{
	/// <summary>
	/// The type of this node.
	/// </summary>
	FeedNodeType Type { get; }
	
	/// <summary>
	/// The feed object for this node.
	/// </summary>
	Feed Feed { get; }
	
	/// <summary>
	/// Child nodes of this node. A <c>null</c> value indicates that this node is a leaf node.
	/// </summary>
	ReadOnlyObservableCollection<IReadOnlyFeedNode>? Children { get; }
	
	/// <summary>
	/// Custom title of the node. If set to <c>null</c>, the feed's name is used.
	/// </summary>
	string? Title { get; }

	/// <summary>
	/// Custom symbol for the node. If set to <c>null</c>, the feed's symbol is used.
	/// </summary>
	Symbol? Symbol { get; }
	
	/// <summary>
	/// The actual title of the feed, using the feed's title as the fallback.
	/// </summary>
	string? ActualTitle { get; }
	
	/// <summary>
	/// The actual symbol of the feed, using the feed's symbol as the fallback.
	/// </summary>
	Symbol? ActualSymbol { get; }

	/// <summary>
	/// Flag indicating whether the user should be able to customize this node.
	/// </summary>
	bool IsUserCustomizable { get; }
}
