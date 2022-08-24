using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using FluentFeeds.App.Shared.EventArgs;

namespace FluentFeeds.App.Shared.Models.Feeds.Loaders;

/// <summary>
/// A feed which combines the items of multiple other feeds.
/// </summary>
public sealed class GroupFeedLoader : FeedLoader
{
	public GroupFeedLoader(IFeedView feed) : base(feed)
	{
		_loaders = GetLoaders();
		foreach (var loader in _loaders)
		{
			loader.ItemsUpdated += HandleItemsUpdated;
		}

		if (feed.Children != null)
		{
			(feed.Children as INotifyCollectionChanged).CollectionChanged += (s, e) => UpdateLoaders();
		}
	}

	private ImmutableHashSet<FeedLoader> GetLoaders()
	{
		return Feed.Children?
			.Where(feed => !feed.IsExcludedFromGroup)
			.Select(feed => feed.Loader)
			.ToImmutableHashSet() ?? ImmutableHashSet<FeedLoader>.Empty;
	}

	private void UpdateLoaders()
	{
		var oldLoaders = _loaders;
		var newLoaders = GetLoaders();
		_loaders = newLoaders;
		
		var added = newLoaders.Except(oldLoaders);
		var removed = oldLoaders.Except(newLoaders);
		
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
