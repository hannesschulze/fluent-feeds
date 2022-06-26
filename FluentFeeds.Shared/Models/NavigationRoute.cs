using System;

namespace FluentFeeds.Shared.Models;

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
	public static NavigationRoute Feed(Feed source) => new(NavigationRouteType.Feed, source);

	/// <summary>
	/// The type of this route.
	/// </summary>
	public NavigationRouteType Type { get; }

	/// <summary>
	/// The source feed of this entry (only available for <see cref="NavigationRouteType.Feed"/> routes).
	/// </summary>
	public Feed? FeedSource { get; }

	private NavigationRoute(NavigationRouteType type, Feed? feedSource)
	{
		Type = type;
		FeedSource = feedSource;
	}

	public override string ToString() =>
		Type switch
		{
			NavigationRouteType.Settings => "Settings",
			NavigationRouteType.Feed=> $"Feed {{ FeedSource = {FeedSource} }}",
			_ => throw new IndexOutOfRangeException()
		};

	public override int GetHashCode() => HashCode.Combine(Type, FeedSource);
	public override bool Equals(object? other) => other is NavigationRoute casted && Equals(casted);
	public bool Equals(NavigationRoute other) => Type == other.Type && FeedSource == other.FeedSource;

	public static bool operator ==(NavigationRoute left, NavigationRoute right) => left.Equals(right);
	public static bool operator !=(NavigationRoute left, NavigationRoute right) => !left.Equals(right);
}
