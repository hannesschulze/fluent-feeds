using System;
using FluentFeeds.Feeds.Base.Nodes;

namespace FluentFeeds.App.Shared.Models.Navigation;

/// <summary>
/// A route on the main page to which the app can navigate.
/// </summary>
public readonly struct MainNavigationRoute : IEquatable<MainNavigationRoute>
{
	/// <summary>
	/// Navigation entry for the app settings.
	/// </summary>
	public static MainNavigationRoute Settings => new(MainNavigationRouteType.Settings, null);

	/// <summary>
	/// Navigation entry for a feed.
	/// </summary>
	public static MainNavigationRoute Feed(IReadOnlyFeedNode node) => new(MainNavigationRouteType.Feed, node);

	/// <summary>
	/// The type of this route.
	/// </summary>
	public MainNavigationRouteType Type { get; }

	/// <summary>
	/// The feed node of this entry (only available for <see cref="MainNavigationRouteType.Feed"/> routes).
	/// </summary>
	public IReadOnlyFeedNode? FeedNode { get; }

	private MainNavigationRoute(MainNavigationRouteType type, IReadOnlyFeedNode? feedNode)
	{
		Type = type;
		FeedNode = feedNode;
	}

	public override string ToString() =>
		Type switch
		{
			MainNavigationRouteType.Settings => "Settings",
			MainNavigationRouteType.Feed=> $"Feed {{ FeedNode = {FeedNode} }}",
			_ => throw new IndexOutOfRangeException()
		};

	public override int GetHashCode() => HashCode.Combine(Type, FeedNode);
	public override bool Equals(object? other) => other is MainNavigationRoute casted && Equals(casted);
	public bool Equals(MainNavigationRoute other) => Type == other.Type && FeedNode == other.FeedNode;

	public static bool operator ==(MainNavigationRoute left, MainNavigationRoute right) => left.Equals(right);
	public static bool operator !=(MainNavigationRoute left, MainNavigationRoute right) => !left.Equals(right);
}
