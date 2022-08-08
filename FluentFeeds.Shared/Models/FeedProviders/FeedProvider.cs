using System;
using System.Threading.Tasks;
using FluentFeeds.Shared.Models.Nodes;

namespace FluentFeeds.Shared.Models.FeedProviders;

/// <summary>
/// A pluggable class which can create feeds and has its own area in the sidebar.
/// </summary>
public abstract class FeedProvider
{
	/// <summary>
	/// Create the initial set of items presented when the user adds this provider and there is no saved structure.
	/// </summary>
	public abstract FeedGroup CreateInitialTree();
	
	/// <summary>
	/// Create a new feed item for the specified URL (if supported).
	/// </summary>
	public abstract Task<FeedItem>? CreateItemAsync(Uri url);
	
	/// <summary>
	/// Load a serialized feed item as returned by <see cref="StoreItem"/>.
	/// </summary>
	public abstract FeedItem LoadItem(string serialized);
	
	/// <summary>
	/// Serialize a feed item so it can be loaded using <see cref="LoadItem"/>.
	/// </summary>
	public abstract string StoreItem(FeedItem item);
}
