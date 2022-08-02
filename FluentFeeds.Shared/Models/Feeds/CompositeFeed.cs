using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

namespace FluentFeeds.Shared.Models.Feeds;

/// <summary>
/// A feed which combines the items of multiple other feeds.
/// </summary>
public sealed class CompositeFeed : Feed
{
	public CompositeFeed(IEnumerable<Feed> feeds)
	{
		Feeds = feeds.ToImmutableArray();

		foreach (var feed in Feeds)
		{
			feed.ItemsUpdated += HandleItemsUpdated;
		}
	}

	public CompositeFeed(params Feed[] feeds) : this(feeds as IEnumerable<Feed>)
	{
	}

	public IReadOnlyList<Feed> Feeds { get; }

	protected override async Task<IEnumerable<Item>> DoLoadAsync()
	{
		_ignoreUpdates = true;
		await Task.WhenAll(Feeds.Select(feed => feed.LoadAsync()));
		_ignoreUpdates = false;

		return GetAllItems();
	}

	protected override async Task<IEnumerable<Item>> DoSynchronizeAsync()
	{
		_ignoreUpdates = true;
		await Task.WhenAll(Feeds.Select(feed => feed.SynchronizeAsync()));
		_ignoreUpdates = false;

		return GetAllItems();
	}

	private void HandleItemsUpdated(object? sender, EventArgs e)
	{
		// Coalesce all updates during load or synchronize into one event.
		if (_ignoreUpdates)
			return;
		
		UpdateItems(GetAllItems());
	}

	private IEnumerable<Item> GetAllItems() => Feeds.SelectMany(feed => feed.Items);

	private bool _ignoreUpdates;
}
