using System;
using FluentFeeds.Common;

namespace FluentFeeds.Feeds.Base.Feeds;

/// <summary>
/// Base class for an immutable descriptor which can be used to create a feed. 
/// </summary>
public abstract class FeedDescriptor : IEquatable<FeedDescriptor>
{
	/// <summary>
	/// The type of this descriptor.
	/// </summary>
	public abstract FeedDescriptorType Type { get; }
	
	/// <summary>
	/// Name of the feed (if any).
	/// </summary>
	public string? Name { get; init; }
	
	/// <summary>
	/// Symbol of the feed (if any).
	/// </summary>
	public Symbol? Symbol { get; init; }

	/// <summary>
	/// Flag indicating if the user should be able to customize (rename, delete, add children) the feed.
	/// </summary>
	public bool IsUserCustomizable { get; init; } = true;

	/// <summary>
	/// If set to true, the feed's content will not be shown in its parent group.
	/// </summary>
	public bool IsContentIgnoredInGroup { get; init; } = false;

	public virtual bool Equals(FeedDescriptor? other)
	{
		if (ReferenceEquals(this, other))
			return true;
		if (ReferenceEquals(null, other))
			return false;
		return
			Type == other.Type && Name == other.Name && Symbol == other.Symbol &&
			IsUserCustomizable == other.IsUserCustomizable && IsContentIgnoredInGroup == other.IsContentIgnoredInGroup;
	}

	public override bool Equals(object? obj)
	{
		if (ReferenceEquals(this, obj))
			return true;
		return obj is FeedDescriptor other && Equals(other);
	}
	
	public override int GetHashCode()
	{
		return HashCode.Combine(Type, Name, Symbol, IsUserCustomizable, IsContentIgnoredInGroup);
	}

	public override string ToString() =>
		$"FeedDescriptor {{ Type = {Type}, Name = {Name}, Symbol = {Symbol}, " +
		$"IsUserCustomizable = {IsUserCustomizable}, IsContentIgnoredInGroup = {IsContentIgnoredInGroup} }}";

	public static bool operator ==(FeedDescriptor? lhs, FeedDescriptor? rhs) => lhs?.Equals(rhs) ?? ReferenceEquals(rhs, null);
	
	public static bool operator !=(FeedDescriptor? lhs, FeedDescriptor? rhs) => !(lhs == rhs);
}
