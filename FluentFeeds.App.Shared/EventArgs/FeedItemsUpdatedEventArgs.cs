using System.Collections.Immutable;
using FluentFeeds.App.Shared.Models.Feeds.Loaders;
using FluentFeeds.App.Shared.Models.Items;

namespace FluentFeeds.App.Shared.EventArgs;

/// <summary>
/// Event args when the items of a <see cref="FeedLoader"/> were updated.
/// </summary>
public sealed class FeedItemsUpdatedEventArgs : System.EventArgs
{
	public FeedItemsUpdatedEventArgs(ImmutableHashSet<IItemView> updatedItems)
	{
		UpdatedItems = updatedItems;
	}

	/// <summary>
	/// The updated set of items.
	/// </summary>
	public ImmutableHashSet<IItemView> UpdatedItems { get; }
}
