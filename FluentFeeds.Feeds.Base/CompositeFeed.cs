using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

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
				UpdateItems();
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

	protected override Task DoLoadAsync()
	{
		_hasStartedLoading = true;
		return CoalesceUpdates(feed => feed.LoadAsync());
	}

	protected override Task DoSynchronizeAsync()
	{
		return CoalesceUpdates(feed => feed.SynchronizeAsync());
	}

	private async Task CoalesceUpdates(Func<Feed, Task> action)
	{
		_ignoreUpdates = true;
		var errors = new List<Exception>();
		foreach (var feed in Feeds)
		{
			try
			{
				await action(feed);
			}
			catch (Exception e)
			{
				errors.Add(e);
			}
		}
		_ignoreUpdates = false;
		UpdateItems();
		
		if (errors.Count != 0)
		{
			throw new AggregateException(errors);
		}
	}

	private void HandleItemsUpdated(object? sender, EventArgs e)
	{
		// Coalesce all updates during load or synchronize into one event.
		if (_ignoreUpdates || !_hasStartedLoading)
			return;
		
		UpdateItems();
	}

	private void UpdateItems() => Items = Feeds.SelectMany(feed => feed.Items).ToImmutableHashSet();

	private bool _ignoreUpdates;
	private bool _hasStartedLoading;
	private ImmutableHashSet<Feed> _feeds;
}
