using System.Collections.Immutable;
using FluentFeeds.Feeds.Base;
using FluentFeeds.Feeds.Base.Items;

namespace FluentFeeds.App.Shared.EventArgs;

/// <summary>
/// Event args when the items of a <see cref="Feed"/> were updated.
/// </summary>
public sealed class FeedItemsUpdatedEventArgs : System.EventArgs
{
	public FeedItemsUpdatedEventArgs(ImmutableHashSet<IReadOnlyStoredItem> updatedItems)
	{
		UpdatedItems = updatedItems;
	}

	/// <summary>
	/// The updated set of items.
	/// </summary>
	public ImmutableHashSet<IReadOnlyStoredItem> UpdatedItems { get; }
}
