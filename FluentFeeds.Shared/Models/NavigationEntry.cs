using System;
using System.Collections.Generic;

namespace FluentFeeds.Shared.Models;

/// <summary>
/// An entry in the navigation back-stack.
/// </summary>
public readonly struct NavigationEntry : IEquatable<NavigationEntry>
{
	/// <summary>
	/// Navigation entry for the app settings.
	/// </summary>
	public static NavigationEntry Settings => new(NavigationEntryType.Settings, null, null);

	/// <summary>
	/// Navigation entry for an optional item in a feed.
	/// </summary>
	public static NavigationEntry FeedItem(Feed feed, Item? item) => new(NavigationEntryType.FeedItem, feed, item);

	/// <summary>
	/// The type of this entry.
	/// </summary>
	public NavigationEntryType Type { get; }

	/// <summary>
	/// The feed of this entry (only available for <see cref="NavigationEntryType.FeedItem"/> entries).
	/// </summary>
	public Feed? Feed { get; }

	/// <summary>
	/// The item of this entry (only optionally available for <see cref="NavigationEntryType.FeedItem"/> entries).
	/// </summary>
	public Item? Item { get; }

	private NavigationEntry(NavigationEntryType type, Feed? feed, Item? item)
	{
		Type = type;
		Feed = feed;
		Item = item;
	}

	public override string ToString() =>
		Type switch
		{
			NavigationEntryType.Settings => "Settings",
			NavigationEntryType.FeedItem => $"FeedItem {{ Feed = {Feed}, Item = {Item} }}",
			_ => throw new IndexOutOfRangeException()
		};

	public override int GetHashCode() => HashCode.Combine(Type, Feed, Item);
	public override bool Equals(object? other) => other is NavigationEntry casted && Equals(casted);
	public bool Equals(NavigationEntry other) => Type == other.Type && Feed == other.Feed && Item == other.Item;

	public static bool operator ==(NavigationEntry left, NavigationEntry right) => left.Equals(right);
	public static bool operator !=(NavigationEntry left, NavigationEntry right) => !left.Equals(right);
}
