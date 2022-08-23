using System;
using FluentFeeds.Feeds.Base.Items;
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
	public static FeedNavigationRoute Placeholder(int selectionCount) =>
		new(FeedNavigationRouteType.Placeholder, selectionCount, null, null);

	/// <summary>
	/// Page indicating loading progress for an item.
	/// </summary>
	public static FeedNavigationRoute Loading(IReadOnlyStoredItem item) =>
		new(FeedNavigationRouteType.Loading, 1, item, null);

	/// <summary>
	/// Page displaying article content for an item.
	/// </summary>
	public static FeedNavigationRoute Article(IReadOnlyStoredItem item, ArticleItemContent content) =>
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
	/// The single item selected (not set for <see cref="FeedNavigationRouteType.Placeholder"/>.
	/// </summary>
	public IReadOnlyStoredItem? Item { get; }
	
	/// <summary>
	/// The loaded content for the item (only set for content routes).
	/// </summary>
	public ItemContent? Content { get; }

	private FeedNavigationRoute(
		FeedNavigationRouteType type, int selectionCount, IReadOnlyStoredItem? item, ItemContent? content)
	{
		Type = type;
		SelectionCount = selectionCount;
		Item = item;
		Content = content;
	}

	public override string ToString() =>
		Type switch
		{
			FeedNavigationRouteType.Placeholder => $"Placeholder {{ SelectionCount = {SelectionCount} }}",
			FeedNavigationRouteType.Loading => $"Loading {{ Item = {Item} }}",
			FeedNavigationRouteType.Article => $"Article {{ Item = {Item}, Content = {Content} }}",
			_ => throw new IndexOutOfRangeException()
		};

	public override int GetHashCode() => HashCode.Combine(Type, SelectionCount, Item, Content);
	public override bool Equals(object? other) => other is FeedNavigationRoute casted && Equals(casted);
	public bool Equals(FeedNavigationRoute other) =>
		Type == other.Type && SelectionCount == other.SelectionCount && Item == other.Item && Content == other.Content;

	public static bool operator ==(FeedNavigationRoute left, FeedNavigationRoute right) => left.Equals(right);
	public static bool operator !=(FeedNavigationRoute left, FeedNavigationRoute right) => !left.Equals(right);
}
