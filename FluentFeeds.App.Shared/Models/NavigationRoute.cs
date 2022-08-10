using System;
using FluentFeeds.Feeds.Base.Nodes;

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
	public static NavigationRoute Feed(IReadOnlyFeedNode node) => new(NavigationRouteType.Feed, node);

	/// <summary>
	/// The type of this route.
	/// </summary>
	public NavigationRouteType Type { get; }

	/// <summary>
	/// The feed node of this entry (only available for <see cref="NavigationRouteType.Feed"/> routes).
	/// </summary>
	public IReadOnlyFeedNode? FeedNode { get; }

	private NavigationRoute(NavigationRouteType type, IReadOnlyFeedNode? feedNode)
	{
		Type = type;
		FeedNode = feedNode;
	}

	public override string ToString() =>
		Type switch
		{
			NavigationRouteType.Settings => "Settings",
			NavigationRouteType.Feed=> $"Feed {{ FeedNode = {FeedNode} }}",
			_ => throw new IndexOutOfRangeException()
		};

	public override int GetHashCode() => HashCode.Combine(Type, FeedNode);
	public override bool Equals(object? other) => other is NavigationRoute casted && Equals(casted);
	public bool Equals(NavigationRoute other) => Type == other.Type && FeedNode == other.FeedNode;

	public static bool operator ==(NavigationRoute left, NavigationRoute right) => left.Equals(right);
	public static bool operator !=(NavigationRoute left, NavigationRoute right) => !left.Equals(right);
}
