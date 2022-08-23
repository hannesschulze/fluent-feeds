namespace FluentFeeds.App.Shared.Models.Navigation;

/// <summary>
/// Type of a <see cref="FeedNavigationRoute"/>.
/// </summary>
public enum FeedNavigationRouteType
{
	/// <summary>
	/// Page showing information about the current item selection (shown if zero or more than one items are selected).
	/// </summary>
	Placeholder,
	/// <summary>
	/// Page indicating loading progress for an item.
	/// </summary>
	Loading,
	/// <summary>
	/// Page displaying article content for an item.
	/// </summary>
	Article
}
