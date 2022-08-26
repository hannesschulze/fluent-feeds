using System;
using FluentFeeds.App.Shared.Models.Items;
using FluentFeeds.Feeds.Base.Items.Content;

namespace FluentFeeds.App.Shared.Models.Navigation;

/// <summary>
/// A route on the feed page to which the app can navigate.
/// </summary>
public readonly struct FeedNavigationRoute : IEquatable<FeedNavigationRoute>
{
	/// <summary>
	/// Page showing information about the current item selection (shown if zero or more than one items are selected).
	/// </summary>
	public static FeedNavigationRoute Selection(int count) =>
		new(FeedNavigationRouteType.Selection, count, null, null, null);

	/// <summary>
	/// Page displaying item with article content.
	/// </summary>
	public static FeedNavigationRoute ArticleItem(IItemView item, ArticleItemContent content) =>
		new(FeedNavigationRouteType.ArticleItem, 1, item, content, null);

	/// <summary>
	/// Page displaying item with comment content.
	/// </summary>
	public static FeedNavigationRoute CommentItem(IItemView item, CommentItemContent content) =>
		new(FeedNavigationRouteType.CommentItem, 1, item, null, content);

	/// <summary>
	/// The type of this route.
	/// </summary>
	public FeedNavigationRouteType Type { get; }

	/// <summary>
	/// The number of items currently selected.
	/// </summary>
	public int SelectionCount { get; }
	
	/// <summary>
	/// The single item selected (not set for <see cref="FeedNavigationRouteType.Selection"/>.
	/// </summary>
	public IItemView? Item { get; }
	
	/// <summary>
	/// The loaded content for the article item (only set for <see cref="FeedNavigationRouteType.ArticleItem"/>).
	/// </summary>
	public ArticleItemContent? ArticleContent { get; }

	/// <summary>
	/// The loaded content for the article item (only set for <see cref="FeedNavigationRouteType.CommentItem"/>).
	/// </summary>
	public CommentItemContent? CommentContent { get; }

	private FeedNavigationRoute(
		FeedNavigationRouteType type, int selectionCount, IItemView? item, ArticleItemContent? articleContent,
		CommentItemContent? commentContent)
	{
		Type = type;
		SelectionCount = selectionCount;
		Item = item;
		ArticleContent = articleContent;
		CommentContent = commentContent;
	}

	public override string ToString() =>
		Type switch
		{
			FeedNavigationRouteType.Selection => $"Selection {{ Count = {SelectionCount} }}",
			FeedNavigationRouteType.ArticleItem => $"ArticleItem {{ Item = {Item}, Content = {ArticleContent} }}",
			FeedNavigationRouteType.CommentItem => $"CommentItem {{ Item = {Item}, Content = {CommentContent} }}",
			_ => throw new IndexOutOfRangeException()
		};

	public override int GetHashCode() => HashCode.Combine(Type, SelectionCount, Item, ArticleContent, CommentContent);
	public override bool Equals(object? other) => other is FeedNavigationRoute casted && Equals(casted);
	public bool Equals(FeedNavigationRoute other) =>
		Type == other.Type && SelectionCount == other.SelectionCount && Item == other.Item &&
		ArticleContent == other.ArticleContent && CommentContent == other.CommentContent;

	public static bool operator ==(FeedNavigationRoute left, FeedNavigationRoute right) => left.Equals(right);
	public static bool operator !=(FeedNavigationRoute left, FeedNavigationRoute right) => !left.Equals(right);
}
