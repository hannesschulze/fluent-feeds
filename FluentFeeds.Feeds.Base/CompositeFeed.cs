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

			if (_hasStartedLoading)
			{
				Items = GetAllItems().ToImmutableHashSet();
				foreach (var feed in added)
				{
					LoadAddedFeed(feed);
				}
			}
		}
	}

	private static async void LoadAddedFeed(Feed feeds)
	{
		try
		{
			await feeds.LoadAsync();
		}
		catch (Exception)
		{
			// Ignore
		}
	}

	protected override async Task<IEnumerable<IReadOnlyStoredItem>> DoLoadAsync()
	{
		_hasStartedLoading = true;
		await CoalesceUpdates(feed => feed.LoadAsync());
		return GetAllItems();
	}

	protected override async Task<IEnumerable<IReadOnlyStoredItem>> DoSynchronizeAsync()
	{
		await CoalesceUpdates(feed => feed.SynchronizeAsync());
		return GetAllItems();
	}

	private async Task CoalesceUpdates(Func<Feed, Task> action)
	{
		_ignoreUpdates = true;
		try
		{
			await Task.WhenAll(Feeds.Select(action));
		}
		catch (Exception)
		{
			_ignoreUpdates = false;
			Items = GetAllItems().ToImmutableHashSet();
			throw;
		}

		_ignoreUpdates = false;
	}

	private void HandleItemsUpdated(object? sender, EventArgs e)
	{
		// Coalesce all updates during load or synchronize into one event.
		if (_ignoreUpdates || !_hasStartedLoading)
			return;
		
		Items = GetAllItems().ToImmutableHashSet();
	}

	private IEnumerable<IReadOnlyStoredItem> GetAllItems() => Feeds.SelectMany(feed => feed.Items);

	private bool _ignoreUpdates;
	private bool _hasStartedLoading;
	private ImmutableHashSet<Feed> _feeds;
}
