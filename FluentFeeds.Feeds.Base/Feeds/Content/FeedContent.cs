using System;
using System.Collections.Immutable;
using System.Linq;
using FluentFeeds.Common.Helpers;
using FluentFeeds.Feeds.Base.Items;

namespace FluentFeeds.Feeds.Base.Feeds.Content;

/// <summary>
/// Updated content of a feed returned by <see cref="IFeedContentLoader"/>.
/// </summary>
public sealed class FeedContent : IEquatable<FeedContent>
{
	public FeedContent()
	{
		Metadata = new FeedMetadata();
		Items = ImmutableArray<ItemDescriptor>.Empty;
	}
	
	public FeedContent(FeedMetadata metadata, params ItemDescriptor[] items)
	{
		Metadata = metadata;
		Items = ImmutableArray.Create(items);
	}
	
	/// <summary>
	/// Metadata for the feed.
	/// </summary>
	public FeedMetadata Metadata { get; init; }
	
	/// <summary>
	/// Descriptors for the items currently in the feed (this includes added and updated items).
	/// </summary>
	public ImmutableArray<ItemDescriptor> Items { get; init; }
	
	public bool Equals(FeedContent? other)
	{
		if (ReferenceEquals(this, other))
			return true;
		if (ReferenceEquals(null, other))
			return false;
		return Metadata == other.Metadata && Items.SequenceEqual(other.Items);
	}

	public override bool Equals(object? obj)
	{
		if (ReferenceEquals(this, obj))
			return true;
		return obj is FeedContent other && Equals(other);
	}
	
	public override int GetHashCode() => HashCode.Combine(Metadata, Items.SequenceHashCode());

	public override string ToString() => $"FeedContent {{ Metadata = {Metadata}, Items = {Items.SequenceString()} }}";

	public static bool operator ==(FeedContent? lhs, FeedContent? rhs) => lhs?.Equals(rhs) ?? ReferenceEquals(rhs, null);
	
	public static bool operator !=(FeedContent? lhs, FeedContent? rhs) => !(lhs == rhs);
}
