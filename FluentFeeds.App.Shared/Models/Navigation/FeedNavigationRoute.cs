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
		new(FeedNavigationRouteType.Selection, count, null, null);

	/// <summary>
	/// Page displaying article content for an item.
	/// </summary>
	public static FeedNavigationRoute Article(IItemView item, ArticleItemContent content) =>
		new(FeedNavigationRouteType.Article, 1, item, content);

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
	/// The loaded content for the article item (only set for <see cref="FeedNavigationRouteType.Article"/>).
	/// </summary>
	public ArticleItemContent? ArticleContent { get; }

	private FeedNavigationRoute(
		FeedNavigationRouteType type, int selectionCount, IItemView? item, ArticleItemContent? articleContent)
	{
		Type = type;
		SelectionCount = selectionCount;
		Item = item;
		ArticleContent = articleContent;
	}

	public override string ToString() =>
		Type switch
		{
			FeedNavigationRouteType.Selection => $"Selection {{ Count = {SelectionCount} }}",
			FeedNavigationRouteType.Article => $"Article {{ Item = {Item}, Content = {ArticleContent} }}",
			_ => throw new IndexOutOfRangeException()
		};

	public override int GetHashCode() => HashCode.Combine(Type, SelectionCount, Item, ArticleContent);
	public override bool Equals(object? other) => other is FeedNavigationRoute casted && Equals(casted);
	public bool Equals(FeedNavigationRoute other) =>
		Type == other.Type && SelectionCount == other.SelectionCount && Item == other.Item &&
		ArticleContent == other.ArticleContent;

	public static bool operator ==(FeedNavigationRoute left, FeedNavigationRoute right) => left.Equals(right);
	public static bool operator !=(FeedNavigationRoute left, FeedNavigationRoute right) => !left.Equals(right);
}
