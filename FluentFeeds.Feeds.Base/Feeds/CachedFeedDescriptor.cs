using System;
using FluentFeeds.Feeds.Base.Feeds.Content;

namespace FluentFeeds.Feeds.Base.Feeds;

/// <summary>
/// Immutable descriptor which can be used to create a cached feed using a <see cref="IFeedContentLoader"/> to load its
/// updated content.
/// </summary>
public sealed class CachedFeedDescriptor : FeedDescriptor
{
	public CachedFeedDescriptor(IFeedContentLoader contentLoader)
	{
		ContentLoader = contentLoader;
	}

	/// <summary>
	/// The loader instance which is used to load the feed's updated content.
	/// </summary>
	public IFeedContentLoader ContentLoader { get; init; }

	/// <summary>
	/// Identifier of the cache used to store the item. If set to null, a new GUID is generated.
	/// </summary>
	public Guid? ItemCacheIdentifier { get; init; } = null;
	
	public override FeedDescriptorType Type => FeedDescriptorType.Cached;
	
	public override string ToString() =>
		$"CachedFeedDescriptor {{ Name = {Name}, Symbol = {Symbol}, IsUserCustomizable = {IsUserCustomizable}, " +
		$"IsContentIgnoredInGroup = {IsContentIgnoredInGroup}, ContentLoader = {ContentLoader}, " +
		$"ItemCacheIdentifier = {ItemCacheIdentifier} }}";

	public override int GetHashCode() => HashCode.Combine(base.GetHashCode(), ContentLoader);

	public override bool Equals(FeedDescriptor? other)
	{
		if (ReferenceEquals(this, other))
			return true;
		if (!base.Equals(other))
			return false;
		return other is CachedFeedDescriptor casted && ContentLoader == casted.ContentLoader;
	}
}
