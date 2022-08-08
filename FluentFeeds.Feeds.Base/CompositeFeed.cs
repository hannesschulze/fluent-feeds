using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using FluentFeeds.Feeds.Base.Items;

namespace FluentFeeds.Feeds.Base;

/// <summary>
/// A feed which combines the items of multiple other feeds.
/// </summary>
public sealed class CompositeFeed : Feed
{
	public CompositeFeed(IEnumerable<Feed> feeds)
	{
		_feeds = feeds.ToImmutableHashSet();

		foreach (var feed in Feeds)
		{
			feed.ItemsUpdated += HandleItemsUpdated;
		}
	}

	public CompositeFeed(params Feed[] feeds) : this(feeds as IEnumerable<Feed>)
	{
	}

	public ImmutableHashSet<Feed> Feeds
	{
		get => _feeds;
		set
		{
			var added = value.Except(_feeds);
			var removed = _feeds.Except(value);
			_feeds = value;

			foreach (var feed in removed)
			{
				feed.ItemsUpdated -= HandleItemsUpdated;
			}
			
			foreach (var feed in added)
			{
				feed.ItemsUpdated += HandleItemsUpdated;
			}
			
			UpdateItems(GetAllItems());
			AddFeeds(added);
		}
	}

	private async void AddFeeds(IEnumerable<Feed> feeds)
	{
		_ignoreUpdates = true;
		await Task.WhenAll(feeds.Select(feed => feed.LoadAsync()));
		_ignoreUpdates = false;
		
		UpdateItems(GetAllItems());
	}

	protected override async Task<IEnumerable<IReadOnlyItem>> DoLoadAsync()
	{
		_ignoreUpdates = true;
		await Task.WhenAll(Feeds.Select(feed => feed.LoadAsync()));
		_ignoreUpdates = false;

		return GetAllItems();
	}

	protected override async Task<IEnumerable<IReadOnlyItem>> DoSynchronizeAsync()
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

	private IEnumerable<IReadOnlyItem> GetAllItems() => Feeds.SelectMany(feed => feed.Items);

	private bool _ignoreUpdates;
	private ImmutableHashSet<Feed> _feeds;
}
