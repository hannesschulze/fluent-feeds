using System.Collections.Generic;
using FluentFeeds.App.Shared.Models.Items;
using FluentFeeds.App.Shared.Models.Storage;

namespace FluentFeeds.App.Shared.EventArgs;

/// <summary>
/// Event args when items were deleted from an <see cref="IItemStorage"/>.
/// </summary>
public sealed class ItemsDeletedEventArgs : System.EventArgs
{
	public ItemsDeletedEventArgs(IReadOnlyCollection<IItemView> items)
	{
		Items = items;
	}

	/// <summary>
	/// The deleted items.
	/// </summary>
	public IReadOnlyCollection<IItemView> Items { get; }
}
