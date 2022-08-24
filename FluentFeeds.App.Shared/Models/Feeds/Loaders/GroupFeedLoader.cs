using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using FluentFeeds.App.Shared.EventArgs;

namespace FluentFeeds.App.Shared.Models.Feeds.Loaders;

/// <summary>
/// A feed which combines the items of multiple other feeds.
/// </summary>
public sealed class GroupFeedLoader : FeedLoader
{
	public GroupFeedLoader(ImmutableHashSet<FeedLoader> loaders)
	{
		_loaders = loaders;
		foreach (var loader in loaders)
		{
			loader.ItemsUpdated += HandleItemsUpdated;
		}
	}

	public ImmutableHashSet<FeedLoader> Loaders
	{
		get => _loaders;
		set
		{
			var oldLoaders = _loaders;
			_loaders = value;
		
			var added = _loaders.Except(oldLoaders);
			var removed = oldLoaders.Except(_loaders);
		
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
				foreach (var loader in added)
				{
					InitializeAddedFeed(loader);
				}
			}
		}
	}

	private static async void InitializeAddedFeed(FeedLoader loader)
	{
		try
		{
			await loader.InitializeAsync();
		}
		catch (Exception)
		{
			// Ignore
		}
	}

	protected override Task DoInitializeAsync()
	{
		_hasStartedLoading = true;
		return CoalesceUpdates(feed => feed.InitializeAsync());
	}

	protected override Task DoSynchronizeAsync()
	{
		return CoalesceUpdates(feed => feed.SynchronizeAsync());
	}

	private async Task CoalesceUpdates(Func<FeedLoader, Task> action)
	{
		_ignoreUpdates = true;
		var errors = new List<Exception>();
		foreach (var loader in _loaders)
		{
			try
			{
				await action(loader);
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

	private void HandleItemsUpdated(object? sender, FeedItemsUpdatedEventArgs e)
	{
		// Coalesce all updates during load or synchronize into one event.
		if (_ignoreUpdates || !_hasStartedLoading)
			return;
		
		UpdateItems();
	}

	private void UpdateItems() => Items = _loaders.SelectMany(loader => loader.Items).ToImmutableHashSet();

	private bool _ignoreUpdates;
	private bool _hasStartedLoading;
	private ImmutableHashSet<FeedLoader> _loaders;
}
