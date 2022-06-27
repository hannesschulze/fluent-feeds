using System;
using System.Collections.Immutable;
using System.Linq;
using FluentFeeds.Shared.RichText.Blocks.List;
using FluentFeeds.Shared.RichText.Helpers;

namespace FluentFeeds.Shared.RichText.Blocks;

/// <summary>
/// An ordered or unordered list containing items which can host other blocks.
/// </summary>
public sealed class ListBlock : Block
{
	/// <summary>
	/// Create a new default-constructed list block.
	/// </summary>
	public ListBlock()
	{
		Items = ImmutableArray<ListItem>.Empty;
	}

	/// <summary>
	/// Create a new list block hosting the provided items.
	/// </summary>
	public ListBlock(params ListItem[] items)
	{
		Items = ImmutableArray.Create(items);
	}

	/// <summary>
	/// The items in this list.
	/// </summary>
	public ImmutableArray<ListItem> Items { get; init; }

	/// <summary>
	/// The style of this list.
	/// </summary>
	public ListStyle Style { get; init; } = ListStyle.Unordered;

	public override BlockType Type => BlockType.List;

	public override void Accept(IBlockVisitor visitor) => visitor.Visit(this);

	public override bool Equals(Block? other)
	{
		if (ReferenceEquals(this, other))
			return true;
		if (!base.Equals(other))
			return false;
		return other is ListBlock casted && Items.SequenceEqual(casted.Items) && Style == casted.Style;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(base.GetHashCode(), Items.SequenceHashCode(), Style);
	}
}
