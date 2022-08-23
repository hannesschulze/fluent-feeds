using System.Collections.Generic;
using FluentFeeds.Feeds.Base.Items;
using FluentFeeds.Feeds.Base.Storage;

namespace FluentFeeds.Feeds.Base.EventArgs;

/// <summary>
/// Event args when items were deleted from an <see cref="IItemStorage"/>.
/// </summary>
public sealed class ItemsDeletedEventArgs : System.EventArgs
{
	public ItemsDeletedEventArgs(IReadOnlyCollection<IReadOnlyStoredItem> items)
	{
		Items = items;
	}

	/// <summary>
	/// The deleted items.
	/// </summary>
	public IReadOnlyCollection<IReadOnlyStoredItem> Items { get; }
}
