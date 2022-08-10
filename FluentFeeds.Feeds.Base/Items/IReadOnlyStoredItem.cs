using System;

namespace FluentFeeds.Feeds.Base.Items;

/// <summary>
/// <para>Read-only view into a persistently stored item.</para>
///
/// <para>This adds a unique identifier and a "read" flag to the base item.</para>
/// </summary>
public interface IReadOnlyStoredItem : IReadOnlyItem
{
	/// <summary>
	/// Unique identifier for this item.
	/// </summary>
	Guid Identifier { get; }

	/// <summary>
	/// Flag indicating whether the item was read.
	/// </summary>
	bool IsRead { get; }
}
