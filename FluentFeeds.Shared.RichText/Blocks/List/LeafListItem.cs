using System;
using System.Collections.Immutable;
using System.Linq;
using FluentFeeds.Shared.RichText.Helpers;
using FluentFeeds.Shared.RichText.Inlines;

namespace FluentFeeds.Shared.RichText.Blocks.List;

/// <summary>
/// A leaf item in a list which hosts a group of inlines.
/// </summary>
public sealed class LeafListItem : ListItem
{
	/// <summary>
	/// Create a new default-constructed leaf list item.
	/// </summary>
	public LeafListItem()
	{
		Inlines = ImmutableArray<Inline>.Empty;
	}

	/// <summary>
	/// Create a new leaf list item hosting the provided inlines.
	/// </summary>
	public LeafListItem(params Inline[] inlines)
	{
		Inlines = ImmutableArray.Create(inlines);
	}

	/// <summary>
	/// The inlines hosted in this list item.
	/// </summary>
	public ImmutableArray<Inline> Inlines { get; init; }

	public override ListItemType Type => ListItemType.Leaf;

	public override void Accept(IListItemVisitor visitor) => visitor.Visit(this);

	public override bool Equals(ListItem? other)
	{
		if (ReferenceEquals(this, other))
			return true;
		if (!base.Equals(other))
			return false;
		return other is LeafListItem casted && Inlines.SequenceEqual(casted.Inlines);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(base.GetHashCode(), Inlines.SequenceHashCode());
	}
}
