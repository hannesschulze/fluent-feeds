using System;

namespace FluentFeeds.Feeds.Base.Items.Content;

/// <summary>
/// Base class for an item's content.
/// </summary>
public abstract class ItemContent : IEquatable<ItemContent>
{
	/// <summary>
	/// The content type.
	/// </summary>
	public abstract ItemContentType Type { get; }

	/// <summary>
	/// Flag indicating if the content can be reloaded.
	/// </summary>
	/// <remarks>
	/// If the content is static, this flag should be set to <c>false</c>. If the user should have the option to update
	/// the content using the content loader because the content might change, it should be set to <c>true</c>.
	/// </remarks>
	public bool IsReloadable { get; init; } = false;
	
	public virtual bool Equals(ItemContent? other)
	{
		if (ReferenceEquals(this, other))
			return true;
		if (ReferenceEquals(null, other))
			return false;
		return Type == other.Type && IsReloadable == other.IsReloadable;
	}

	public override bool Equals(object? obj)
	{
		if (ReferenceEquals(this, obj))
			return true;
		return obj is ItemContent other && Equals(other);
	}

	public override int GetHashCode() => HashCode.Combine(Type, IsReloadable);

	public override string ToString() => $"ItemContent {{ Type = {Type}, IsReloadable = {IsReloadable} }}";

	public static bool operator ==(ItemContent? lhs, ItemContent? rhs) => lhs?.Equals(rhs) ?? ReferenceEquals(rhs, null);
	
	public static bool operator !=(ItemContent? lhs, ItemContent? rhs) => !(lhs == rhs);
}
