using System;
using System.Collections.Immutable;
using System.Threading.Tasks;
using FluentFeeds.App.Shared.EventArgs;
using FluentFeeds.App.Shared.Models.Items;

namespace FluentFeeds.App.Shared.Models.Feeds.Loaders;

/// <summary>
/// A feed loader which keeps track of the items in a feed.
/// </summary>
public abstract class FeedLoader
{
	protected FeedLoader(IFeedView feed)
	{
		Feed = feed;
		_initialize = new Lazy<Task>(DoInitializeAsync);
	}
	
	/// <summary>
	/// Event called when <see cref="Items"/> has been updated. This event is usually raised after calling either
	/// <see cref="InitializeAsync"/> or <see cref="SynchronizeAsync"/>.
	/// </summary>
	public event EventHandler<FeedItemsUpdatedEventArgs>? ItemsUpdated;
	
	/// <summary>
	/// The feed which created this loader.
	/// </summary>
	public IFeedView Feed { get; }
	
	/// <summary>
	/// The timestamp at which this feed was last synchronized in the current object's lifetime.
	/// </summary>
	public DateTimeOffset? LastSynchronized { get; private set; }

	/// <summary>
	/// Current snapshot of items provided by the feed.
	/// </summary>
	public ImmutableHashSet<IItemView> Items
	{
		get => _items;
		protected set
		{
			_items = value;
			ItemsUpdated?.Invoke(this, new FeedItemsUpdatedEventArgs(value));
		}
	}

	/// <summary>
	/// <para>Asynchronously load the initial selection of items. The result might be out of date and need to be
	/// synchronized.</para>
	///
	/// <para>This class ensures that there is only one load request for the whole lifetime of the feed. Subsequent
	/// calls either return the same task or a completed task.</para>
	/// </summary>
	public Task InitializeAsync()
	{
		return _initialize.Value;
	}

	/// <summary>
	/// <para>Fetch an up to date list of items from a remote server and update <see cref="Items"/>.</para>
	///
	/// <para>This class ensures that there is only one synchronization operation at a time.</para>
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
	/// Asynchronously update the list of items to the initial selection of items which might be out of date.
	/// </summary>
	protected abstract Task DoInitializeAsync();
	
	/// <summary>
	/// Update the list of items to an up-to-date list fetched from a remote server. It is ensured that
	/// <see cref="DoInitializeAsync"/> has been called before this method.
	/// </summary>
	protected abstract Task DoSynchronizeAsync();

	private async Task SynchronizeAsyncCore()
	{
		await InitializeAsync();
		await DoSynchronizeAsync();

		_isSynchronizing = false;
		LastSynchronized = DateTimeOffset.UtcNow;
	}
	
	private ImmutableHashSet<IItemView> _items = ImmutableHashSet<IItemView>.Empty;
	private Lazy<Task> _initialize;
	private bool _isSynchronizing;
	private Task? _synchronizeTask;
}
