using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentFeeds.Feeds.Base.Items;

namespace FluentFeeds.Feeds.Base.Storage;

/// <summary>
/// Storage abstraction for caching <see cref="IReadOnlyItem"/> objects.
/// </summary>
public interface IItemStorage
{
	/// <summary>
	/// Return all saved items in this storage.
	/// </summary>
	Task<IEnumerable<IReadOnlyStoredItem>> GetItemsAsync();

	/// <summary>
	/// Save the provided set of items or update them if they are already saved in the storage and have changed.
	/// </summary>
	/// <returns>
	/// Stored representations of all items.
	/// </returns>
	Task<IEnumerable<IReadOnlyStoredItem>> AddItemsAsync(IEnumerable<IReadOnlyItem> items);
}
