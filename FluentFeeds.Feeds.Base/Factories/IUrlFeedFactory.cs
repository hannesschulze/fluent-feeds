using System;
using FluentFeeds.Feeds.Base.Feeds.Content;

namespace FluentFeeds.Feeds.Base.Factories;

/// <summary>
/// A factory for creating feed content loaders from a URL.
/// </summary>
public interface IUrlFeedFactory
{
	/// <summary>
	/// Create a content loader which loads feed content from the provided URL.
	/// </summary>
	IFeedContentLoader Create(Uri url);
}
