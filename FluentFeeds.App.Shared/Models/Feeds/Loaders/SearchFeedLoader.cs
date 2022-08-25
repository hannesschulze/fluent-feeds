using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using FluentFeeds.App.Shared.Models.Items;

namespace FluentFeeds.App.Shared.Models.Feeds.Loaders;

/// <summary>
/// A feed loader wrapping another feed loader and searching for keywords.
/// </summary>
public sealed class SearchFeedLoader : FeedLoader
{
	/// <summary>
	/// Immutable snapshot of an item to avoid synchronization.
	/// </summary>
	private readonly record struct SearchableItem(IItemView Item, string Title, string? Author, string? Summary);

	public SearchFeedLoader(FeedLoader source)
	{
		Source = source;
		Source.ItemsUpdated += (s, e) => UpdateItems();
		UpdateItems();
	}

	/// <summary>
	/// The source feed loader which is being searched.
	/// </summary>
	public FeedLoader Source { get; }

	/// <summary>
	/// The search term strings.
	/// </summary>
	/// <remarks>
	/// The result contains all items where every search time can be found in at least one property (out of title,
	/// author and summary).
	/// </remarks>
	public ImmutableArray<string> SearchTerms
	{
		get => _searchTerms;
		set
		{
			if (!_searchTerms.SequenceEqual(value))
			{
				_searchTerms = value;
				UpdateItems();
			}
		}
	}

	public override DateTimeOffset? LastSynchronized => Source.LastSynchronized;

	protected override Task DoInitializeAsync() => Source.InitializeAsync();

	protected override Task DoSynchronizeAsync() => Source.SynchronizeAsync();

	private static bool MatchesSearchTerm(in SearchableItem item, string searchTerm)
	{
		return
			item.Title.Contains(searchTerm, StringComparison.CurrentCultureIgnoreCase) ||
			(item.Author != null && item.Author.Contains(searchTerm, StringComparison.CurrentCultureIgnoreCase)) ||
			(item.Summary != null && item.Summary.Contains(searchTerm, StringComparison.CurrentCultureIgnoreCase));
	}

	private static ImmutableHashSet<IItemView> SearchAsync(
		IReadOnlyList<SearchableItem> items, ImmutableArray<string> searchTerms)
	{
		IEnumerable<int> remainingIndices = Enumerable.Range(0, items.Count).ToImmutableHashSet();

		foreach (var searchTerm in searchTerms)
		{
			var newIndices = new List<int>();

			foreach (var i in remainingIndices)
			{
				if (MatchesSearchTerm(items[i], searchTerm))
				{
					newIndices.Add(i);
				}
			}

			remainingIndices = newIndices;
		}

		return remainingIndices.Select(i => items[i].Item).ToImmutableHashSet();
	}

	private async void UpdateItems()
	{
		_updateItemsToken = null;
		if (SearchTerms.IsEmpty || Source.Items.IsEmpty)
		{
			Items = Source.Items;
			return;
		}

		var token = new object();
		_updateItemsToken = token;

		var searchableItems = Source.Items
			.Select(item => new SearchableItem(item, item.Title, item.Author, item.Summary))
			.ToList();
		var result = await Task.Run(() => SearchAsync(searchableItems, SearchTerms));

		if (_updateItemsToken == token)
		{
			Items = result;
		}
	}

	private ImmutableArray<string> _searchTerms = ImmutableArray<string>.Empty;
	private object? _updateItemsToken;
}
