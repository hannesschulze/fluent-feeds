using System;
using FluentFeeds.App.Shared.Models.Nodes;

namespace FluentFeeds.App.Shared.Models;

/// <summary>
/// A route to which the app can navigate.
/// </summary>
public readonly struct NavigationRoute : IEquatable<NavigationRoute>
{
	/// <summary>
	/// Navigation entry for the app settings.
	/// </summary>
	public static NavigationRoute Settings => new(NavigationRouteType.Settings, null);

	/// <summary>
	/// Navigation entry for a feed.
	/// </summary>
	public static NavigationRoute Feed(FeedItem item) => new(NavigationRouteType.Feed, item);

	/// <summary>
	/// The type of this route.
	/// </summary>
	public NavigationRouteType Type { get; }

	/// <summary>
	/// The feed item of this entry (only available for <see cref="NavigationRouteType.Feed"/> routes).
	/// </summary>
	public FeedItem? FeedItem { get; }

	private NavigationRoute(NavigationRouteType type, FeedItem? feedItem)
	{
		Type = type;
		FeedItem = feedItem;
	}

	public override string ToString() =>
		Type switch
		{
			NavigationRouteType.Settings => "Settings",
			NavigationRouteType.Feed=> $"Feed {{ FeedItem = {FeedItem} }}",
			_ => throw new IndexOutOfRangeException()
		};

	public override int GetHashCode() => HashCode.Combine(Type, FeedItem);
	public override bool Equals(object? other) => other is NavigationRoute casted && Equals(casted);
	public bool Equals(NavigationRoute other) => Type == other.Type && FeedItem == other.FeedItem;

	public static bool operator ==(NavigationRoute left, NavigationRoute right) => left.Equals(right);
	public static bool operator !=(NavigationRoute left, NavigationRoute right) => !left.Equals(right);
}
