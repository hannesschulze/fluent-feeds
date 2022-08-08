using System;

namespace FluentFeeds.Feeds.Base.Storage;

/// <summary>
/// Storage abstraction for feed node objects.
/// </summary>
public interface IFeedStorage
{
	/// <summary>
	/// Return a storage object containing the items for a feed.
	/// </summary>
	IItemStorage GetItemStorage(Guid identifier);
}
