using System;
using FluentFeeds.App.Shared.Models.Feeds;

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
	public static MainNavigationRoute Feed(IFeedView selectedFeed) => new(MainNavigationRouteType.Feed, selectedFeed);

	/// <summary>
	/// The type of this route.
	/// </summary>
	public MainNavigationRouteType Type { get; }

	/// <summary>
	/// The feed of this entry (only available for <see cref="MainNavigationRouteType.Feed"/> routes).
	/// </summary>
	public IFeedView? SelectedFeed { get; }

	private MainNavigationRoute(MainNavigationRouteType type, IFeedView? selectedFeed)
	{
		Type = type;
		SelectedFeed = selectedFeed;
	}

	public override string ToString() =>
		Type switch
		{
			MainNavigationRouteType.Settings => "Settings",
			MainNavigationRouteType.Feed=> $"Feed {{ SelectedFeed = {SelectedFeed} }}",
			_ => throw new IndexOutOfRangeException()
		};

	public override int GetHashCode() => HashCode.Combine(Type, SelectedFeed);
	public override bool Equals(object? other) => other is MainNavigationRoute casted && Equals(casted);
	public bool Equals(MainNavigationRoute other) => Type == other.Type && SelectedFeed == other.SelectedFeed;

	public static bool operator ==(MainNavigationRoute left, MainNavigationRoute right) => left.Equals(right);
	public static bool operator !=(MainNavigationRoute left, MainNavigationRoute right) => !left.Equals(right);
}
