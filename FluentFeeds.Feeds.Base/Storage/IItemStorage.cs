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
	/// Return all saved items.
	/// </summary>
	Task<IEnumerable<IReadOnlyStoredItem>> GetItemsAsync();

	/// <summary>
	/// Save the provided set of items.
	/// </summary>
	Task<IEnumerable<IReadOnlyStoredItem>> AddItemsAsync(IEnumerable<IReadOnlyItem> items);

	/// <summary>
	/// Update a saved item with the specified item to match <c>updatedItem</c>.
	/// </summary>
	Task<IReadOnlyStoredItem> UpdateItemAsync(Guid identifier, IReadOnlyItem updatedItem);
}
