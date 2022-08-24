using System;
using System.Collections.Immutable;
using System.Linq;
using FluentFeeds.Common;
using FluentFeeds.Common.Helpers;

namespace FluentFeeds.Feeds.Base.Feeds;

/// <summary>
/// Immutable descriptor which can be used to create a group feed combining the items from its child feeds.
/// </summary>
public sealed class GroupFeedDescriptor : FeedDescriptor
{
	public GroupFeedDescriptor()
	{
		Children = ImmutableArray<FeedDescriptor>.Empty;
	}
	
	public GroupFeedDescriptor(string? name, Symbol? symbol, params FeedDescriptor[] children)
	{
		Name = name;
		Symbol = symbol;
		Children = ImmutableArray.Create(children);
	}
	
	/// <summary>
	/// Descriptors for the group's child feeds.
	/// </summary>
	public ImmutableArray<FeedDescriptor> Children { get; init; }

	public override FeedDescriptorType Type => FeedDescriptorType.Group;
	
	public override string ToString() =>
		$"GroupFeedDescriptor {{ Name = {Name}, Symbol = {Symbol}, IsUserCustomizable = {IsUserCustomizable}, " +
		$"IsExcludedFromGroup = {IsExcludedFromGroup}, Children = {Children.SequenceString()} }}";

	public override int GetHashCode() => HashCode.Combine(base.GetHashCode(), Children.SequenceHashCode());

	public override bool Equals(FeedDescriptor? other)
	{
		if (ReferenceEquals(this, other))
			return true;
		if (!base.Equals(other))
			return false;
		return other is GroupFeedDescriptor casted && Children.SequenceEqual(casted.Children);
	}
}
