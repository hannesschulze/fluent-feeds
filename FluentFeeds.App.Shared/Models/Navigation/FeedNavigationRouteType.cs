namespace FluentFeeds.App.Shared.Models.Navigation;

/// <summary>
/// Type of a <see cref="FeedNavigationRoute"/>.
/// </summary>
public enum FeedNavigationRouteType
{
	/// <summary>
	/// Page showing information about the current item selection (shown if zero or more than one items are selected).
	/// </summary>
	Selection,
	/// <summary>
	/// Page displaying an item with article content.
	/// </summary>
	ArticleItem,
	/// <summary>
	/// Page displaying an item with comment content.
	/// </summary>
	CommentItem
}
