using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentFeeds.App.Shared.EventArgs;
using FluentFeeds.App.Shared.Models.Items;
using FluentFeeds.Feeds.Base.Items;

namespace FluentFeeds.App.Shared.Models.Storage;

/// <summary>
/// Storage abstraction for caching <see cref="Item"/> objects.
/// </summary>
public interface IItemStorage
{
	/// <summary>
	/// The storage's identifier.
	/// </summary>
	Guid Identifier { get; }
	
	/// <summary>
	/// Event raised when items were permanently deleted from the storage.
	/// </summary>
	event EventHandler<ItemsDeletedEventArgs>? ItemsDeleted;

	/// <summary>
	/// Return all saved items in this storage associated with a given feed.
	/// </summary>
	Task<IEnumerable<IItemView>> GetItemsAsync(Guid feedIdentifier);

	/// <summary>
	/// Save the provided set of items and associate them with a given feed or update them if they are already saved in
	/// the storage and have changed.
	/// </summary>
	/// <returns>
	/// Stored representations of the input items.
	/// </returns>
	Task<IEnumerable<IItemView>> AddItemsAsync(IEnumerable<ItemDescriptor> items, Guid feedIdentifier);

	/// <summary>
	/// Mark an item as read/unread.
	/// </summary>
	Task<IItemView> SetItemReadAsync(Guid identifier, bool isRead);

	/// <summary>
	/// Delete the provided items from the storage.
	/// </summary>
	Task DeleteItemsAsync(IReadOnlyCollection<Guid> identifiers);
}
