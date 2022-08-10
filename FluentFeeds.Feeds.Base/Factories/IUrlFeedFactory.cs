using System;
using System.Threading.Tasks;

namespace FluentFeeds.Feeds.Base.Factories;

/// <summary>
/// A factory for creating feed items from a URL.
/// </summary>
public interface IUrlFeedFactory
{
	/// <summary>
	/// Asynchronously try to create a feed for the provided URL.
	/// </summary>
	Task<Feed> CreateAsync(Uri url);
}
