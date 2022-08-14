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
	protected FeedProvider(FeedProviderMetadata metadata)
	{
		Metadata = metadata;
	}
	
	/// <summary>
	/// Metadata for this feed provider.
	/// </summary>
	public FeedProviderMetadata Metadata { get; }

	/// <summary>
	/// Factory for creating feeds from URLs.
	/// </summary>
	public IUrlFeedFactory? UrlFeedFactory { get; protected set; }
	
	/// <summary>
	/// Create the initial set of feed nodes presented when the user adds this provider and there is no saved tree.
	/// </summary>
	public abstract IReadOnlyFeedNode CreateInitialTree();
	
	/// <summary>
	/// Load a serialized feed as returned by <see cref="StoreFeedAsync"/>.
	/// </summary>
	public abstract Task<Feed> LoadFeedAsync(IFeedStorage feedStorage, string serialized);
	
	/// <summary>
	/// Serialize a feed so it can be loaded using <see cref="LoadFeedAsync"/>.
	/// </summary>
	public abstract Task<string> StoreFeedAsync(Feed feed);
}
