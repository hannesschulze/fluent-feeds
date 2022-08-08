using System;
using System.Threading.Tasks;
using FluentFeeds.Feeds.Base.Factories;
using FluentFeeds.Feeds.Base.Nodes;
using FluentFeeds.Feeds.Base.Storage;

namespace FluentFeeds.Feeds.Base;

/// <summary>
/// A pluggable class which can create feeds and has its own area in the sidebar.
/// </summary>
public abstract class FeedProvider
{
	protected FeedProvider(IFeedStorage storage)
	{
		Storage = storage;
	}
	
	/// <summary>
	/// Persistent feed storage for this feed provider.
	/// </summary>
	public IFeedStorage Storage { get; }
	
	/// <summary>
	/// Factory for creating feeds from URLs.
	/// </summary>
	public abstract IUrlFeedFactory? UrlFeedFactory { get; }
	
	/// <summary>
	/// Create the initial set of nodes presented when the user adds this provider and there is no saved structure.
	/// </summary>
	public abstract FeedGroupNode CreateInitialGroup();
	
	/// <summary>
	/// Load a serialized feed as returned by <see cref="StoreFeed"/>.
	/// </summary>
	public abstract FeedLeafNode LoadFeed(Guid identifier, string serialized);
	
	/// <summary>
	/// Serialize a feed so it can be loaded using <see cref="LoadFeed"/>.
	/// </summary>
	public abstract string StoreFeed(IReadOnlyFeedLeafNode node);
}
