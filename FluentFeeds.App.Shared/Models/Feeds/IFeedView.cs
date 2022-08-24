using System;
using System.Collections.ObjectModel;
using FluentFeeds.App.Shared.Models.Feeds.Loaders;
using FluentFeeds.App.Shared.Models.Storage;
using FluentFeeds.Common;
using FluentFeeds.Feeds.Base.Feeds.Content;

namespace FluentFeeds.App.Shared.Models.Feeds;

/// <summary>
/// Read-only view into a feed.
/// </summary>
public interface IFeedView
{
	/// <summary>
	/// Unique identifier for this feed.
	/// </summary>
	Guid Identifier { get; }
	
	/// <summary>
	/// Storage managing the feed (if it is stored).
	/// </summary>
	IFeedStorage? Storage { get; }

	/// <summary>
	/// Loader managing the feed's items.
	/// </summary>
	FeedLoader Loader { get; }
	
	/// <summary>
	/// Child feeds of this feed. A <c>null</c> value indicates that this node is a leaf node.
	/// </summary>
	ReadOnlyObservableCollection<IFeedView>? Children { get; }
		
	/// <summary>
	/// The parent node for this feed.
	/// </summary>
	IFeedView? Parent { get; }

	/// <summary>
	/// Custom name for the feed. If set to <c>null</c>, the name from the metadata is used.
	/// </summary>
	string? Name { get; }

	/// <summary>
	/// Custom symbol for the feed. If set to <c>null</c>, the symbol from the metadata is used.
	/// </summary>
	Symbol? Symbol { get; }
	
	/// <summary>
	/// Current feed metadata.
	/// </summary>
	FeedMetadata Metadata { get; }

	/// <summary>
	/// The displayed name of the feed, using the name from the metadata as the fallback.
	/// </summary>
	string DisplayName { get; }
	
	/// <summary>
	/// The displayed symbol of the feed, using the symbol from the metadata as the fallback.
	/// </summary>
	Symbol DisplaySymbol { get; }

	/// <summary>
	/// Flag indicating whether the user should be able to customize this node.
	/// </summary>
	bool IsUserCustomizable { get; }
	
	/// <summary>
	/// If set to true, the feed's content will not be shown in its parent group.
	/// </summary>
	bool IsExcludedFromGroup { get; }
}
