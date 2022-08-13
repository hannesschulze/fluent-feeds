using System;
using System.Threading.Tasks;
using FluentFeeds.Feeds.Base.Storage;

namespace FluentFeeds.Feeds.Base.Factories;

/// <summary>
/// A factory for creating feeds from a URL.
/// </summary>
public interface IUrlFeedFactory
{
	/// <summary>
	/// Asynchronously try to create a feed for the provided URL.
	/// </summary>
	Task<Feed> CreateAsync(IFeedStorage feedStorage, Uri url);
}
