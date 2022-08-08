using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentFeeds.Feeds.Base.Items;

namespace FluentFeeds.Feeds.Base.Storage;

/// <summary>
/// Storage abstraction for caching <see cref="Item"/> objects.
/// </summary>
public interface IItemStorage
{
	/// <summary>
	/// Return all saved items associated with the specified feed identifier.
	/// </summary>
	Task<IEnumerable<IReadOnlyItem>> GetItemsAsync(Guid feedIdentifier);

	/// <summary>
	/// Save the provided set of items and associate them with the specified feed identifier.
	/// </summary>
	Task AddItemsAsync(IEnumerable<Item> items, Guid feedIdentifier);

	/// <summary>
	/// Update a saved item with the specified item to match <c>updatedItem</c>.
	/// </summary>
	Task UpdateItemAsync(Guid identifier, Item updatedItem);
}
