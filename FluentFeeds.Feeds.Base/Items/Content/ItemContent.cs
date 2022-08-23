using System;

namespace FluentFeeds.Feeds.Base.Items.Content;

/// <summary>
/// The content of an item in a particular form.
/// </summary>
public abstract class ItemContent : IEquatable<ItemContent>
{
	protected ItemContent(bool isReloadable)
	{
		IsReloadable = isReloadable;
	}
	
	/// <summary>
	/// Accept a visitor on this content.
	/// </summary>
	public abstract void Accept(IItemContentVisitor visitor);
	
	/// <summary>
	/// The content type.
	/// </summary>
	public abstract ItemContentType Type { get; }
	
	/// <summary>
	/// Flag indicating if the content can be reloaded.
	/// </summary>
	/// <remarks>
	/// If the content is static, this flag should be set to <c>false</c>. If the user should have the option to update
	/// the content using the content loader because the content may have changed, it should be set to <c>true</c>.
	/// </remarks>
	public bool IsReloadable { get; }
	
	public virtual bool Equals(ItemContent? other)
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
		return obj is ItemContent other && Equals(other);
	}

	public override int GetHashCode() => HashCode.Combine(Type);

	public override string ToString() => $"ItemContent {{ Type = {Type} }}";

	public static bool operator ==(ItemContent? lhs, ItemContent? rhs) =>
		lhs?.Equals(rhs) ?? ReferenceEquals(rhs, null);
	public static bool operator !=(ItemContent? lhs, ItemContent? rhs) => !(lhs == rhs);
}
