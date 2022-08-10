using System;

namespace FluentFeeds.Feeds.Base.Storage;

/// <summary>
/// Storage abstraction for feed node objects.
/// </summary>
public interface IFeedStorage
{
	/// <summary>
	/// Return the item storage with the specified identifier.
	/// </summary>
	IItemStorage GetItemStorage(Guid identifier);
}
