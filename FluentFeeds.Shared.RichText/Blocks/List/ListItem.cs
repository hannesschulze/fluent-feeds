using System;
using System.Collections.Immutable;
using System.Linq;
using FluentFeeds.Shared.RichText.Helpers;

namespace FluentFeeds.Shared.RichText.Blocks.List;

/// <summary>
/// An item in a list which can contain multiple blocks.
/// <seealso cref="ListBlock"/>
/// </summary>
public sealed class ListItem : IEquatable<ListItem>
{
	/// <summary>
	/// Create a new default-constructed list item.
	/// </summary>
	public ListItem()
	{
		Blocks = ImmutableArray<Block>.Empty;
	}

	/// <summary>
	/// Create a new list item from a list of blocks.
	/// </summary>
	public ListItem(params Block[] blocks)
	{
		Blocks = ImmutableArray.Create(blocks);
	}

	/// <summary>
	/// The list item's content.
	/// </summary>
	public ImmutableArray<Block> Blocks { get; init; }

	public bool Equals(ListItem? other)
	{
		if (ReferenceEquals(this, other))
			return true;
		if (ReferenceEquals(null, other))
			return false;
		return Blocks.SequenceEqual(other.Blocks);
	}

	public override bool Equals(object? obj)
	{
		if (ReferenceEquals(this, obj))
			return true;
		return obj is RichText other && Equals(other);
	}

	public override int GetHashCode()
	{
		return Blocks.SequenceHashCode();
	}

	public static bool operator ==(ListItem lhs, ListItem rhs) => lhs.Equals(rhs);
	public static bool operator !=(ListItem lhs, ListItem rhs) => !lhs.Equals(rhs);
}
