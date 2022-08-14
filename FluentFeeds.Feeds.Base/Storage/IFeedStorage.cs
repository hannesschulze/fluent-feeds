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
	/// <param name="identifier">Storage identifier.</param>
	/// <param name="contentSerializer">Custom serializer for item content loaders.</param>
	IItemStorage GetItemStorage(Guid identifier, IItemContentSerializer? contentSerializer = null);
}
