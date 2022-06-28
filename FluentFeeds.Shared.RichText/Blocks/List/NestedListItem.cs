using System;
using System.Collections.Immutable;
using System.Linq;
using FluentFeeds.Shared.RichText.Helpers;

namespace FluentFeeds.Shared.RichText.Blocks.List;

/// <summary>
/// A nested list inside another list which can contain more list items.
/// </summary>
public sealed class NestedListItem : ListItem
{
	/// <summary>
	/// Create a new default-constructed nested list item.
	/// </summary>
	public NestedListItem()
	{
		Items = ImmutableArray<ListItem>.Empty;
	}

	/// <summary>
	/// Create a new nested list item hosting the provided items.
	/// </summary>
	public NestedListItem(params ListItem[] items)
	{
		Items = ImmutableArray.Create(items);
	}

	/// <summary>
	/// The items in this nested list.
	/// </summary>
	public ImmutableArray<ListItem> Items { get; init; }
	
	/// <summary>
	/// The style of the nested list.
	/// </summary>
	public ListStyle Style { get; init; } = ListStyle.Unordered;

	public override ListItemType Type => ListItemType.Nested;

	public override void Accept(IListItemVisitor visitor) => visitor.Visit(this);

	public override bool Equals(ListItem? other)
	{
		if (ReferenceEquals(this, other))
			return true;
		if (!base.Equals(other))
			return false;
		return other is NestedListItem casted && Items.SequenceEqual(casted.Items);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(base.GetHashCode(), Items.SequenceHashCode());
	}
}
