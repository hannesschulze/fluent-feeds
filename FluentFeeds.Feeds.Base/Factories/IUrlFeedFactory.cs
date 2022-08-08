using System;
using System.Threading.Tasks;
using FluentFeeds.Feeds.Base.Nodes;

namespace FluentFeeds.Feeds.Base.Factories;

/// <summary>
/// A factory for creating feed items from a URL.
/// </summary>
public interface IUrlFeedFactory
{
	/// <summary>
	/// Asynchronously try to create a feed for the provided URL.
	/// </summary>
	Task<FeedLeafNode> CreateAsync(Uri url);
}
