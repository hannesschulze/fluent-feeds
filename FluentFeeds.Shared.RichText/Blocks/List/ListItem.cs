using System;
using System.Collections.Immutable;
using System.Linq;
using FluentFeeds.Shared.RichText.Helpers;

namespace FluentFeeds.Shared.RichText.Blocks.List;

/// <summary>
/// Base class for items in a list.
/// <seealso cref="ListBlock"/>
/// </summary>
public abstract class ListItem : IEquatable<ListItem>
{
	/// <summary>
	/// The type of this list item.
	/// </summary>
	public abstract ListItemType Type { get; }

	/// <summary>
	/// Accept a visitor for this list item.
	/// </summary>
	public abstract void Accept(IListItemVisitor visitor);

	public virtual bool Equals(ListItem? other)
	{
		if (ReferenceEquals(this, other))
			return true;
		if (ReferenceEquals(null, other))
			return false;
		return true;
	}

	public override bool Equals(object? obj)
	{
		if (ReferenceEquals(this, obj))
			return true;
		return obj is ListItem other && Equals(other);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(Type);
	}

	public static bool operator ==(ListItem lhs, ListItem rhs) => lhs.Equals(rhs);
	public static bool operator !=(ListItem lhs, ListItem rhs) => !lhs.Equals(rhs);
}
