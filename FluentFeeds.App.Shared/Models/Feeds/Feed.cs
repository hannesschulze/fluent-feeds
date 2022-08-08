using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using FluentFeeds.App.Shared.Models.Items;

namespace FluentFeeds.App.Shared.Models.Feeds;

/// <summary>
/// A feed which manages a set of items and can be synchronized with a remote server.
///
/// The items in the feed are not sorted. This is to allow for easy chaining of feeds without needing to keep track of
/// the order. The set is sorted and converted to a list only in the final stage.
/// </summary>
public abstract class Feed
{
	/// <summary>
	/// Event called when <see cref="Items"/> has been updated. This event is only raised after calling either
	/// <see cref="LoadAsync"/> or <see cref="SynchronizeAsync"/>.
	/// </summary>
	public event EventHandler? ItemsUpdated;

	/// <summary>
	/// Current snapshot of items provided by the feed.
	/// </summary>
	public ImmutableHashSet<Item> Items { get; private set; } = ImmutableHashSet<Item>.Empty;

	/// <summary>
	/// Asynchronously load the initial selection of items. The result might be out of date and need to be synchronized.
	///
	/// This class ensures that there is only one load request for the whole lifetime of the feed. Subsequent calls
	/// either return the same task or a completed task.
	/// </summary>
	public Task LoadAsync()
	{
		if (_isLoaded)
			return Task.CompletedTask;
		
		_loadTask ??= LoadAsyncCore();
		return _loadTask;
	}

	/// <summary>
	/// Fetch an up to date list of items from a remote server and update <see cref="Items"/>.
	///
	/// This class ensures that there is only one synchronization operation at a time.
	/// </summary>
	public Task SynchronizeAsync()
	{
		if (_synchronizeTask == null || !_isSynchronizing)
		{
			_isSynchronizing = true;
			_synchronizeTask = SynchronizeAsyncCore();
		}
		
		return _synchronizeTask;
	}

	/// <summary>
	/// Asynchronously return the initial selection of items which might be out of date.
	/// </summary>
	protected abstract Task<IEnumerable<Item>> DoLoadAsync();
	
	/// <summary>
	/// Return a list of up to date items fetched from a remote server. It is ensured that <see cref="DoLoadAsync"/> has
	/// been called before this method.
	/// </summary>
	protected abstract Task<IEnumerable<Item>> DoSynchronizeAsync();
	
	/// <summary>
	/// Manually update the list of items.
	/// </summary>
	protected void UpdateItems(IEnumerable<Item> items)
	{
		Items = items.ToImmutableHashSet();
		ItemsUpdated?.Invoke(this, EventArgs.Empty);
	}

	private async Task LoadAsyncCore()
	{
		UpdateItems(await DoLoadAsync());
		
		_isLoaded = true;
	}

	private async Task SynchronizeAsyncCore()
	{
		await LoadAsync();

		UpdateItems(await DoSynchronizeAsync());

		_isSynchronizing = false;
	}

	private bool _isLoaded;
	private bool _isSynchronizing;
	private Task? _loadTask;
	private Task? _synchronizeTask;
}
