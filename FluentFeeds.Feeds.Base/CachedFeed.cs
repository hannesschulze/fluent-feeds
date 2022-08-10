using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using FluentFeeds.Feeds.Base.Items;
using FluentFeeds.Feeds.Base.Storage;

namespace FluentFeeds.Feeds.Base;

/// <summary>
/// A feed subclasses which loads and saves items into an <see cref="IItemStorage"/>.
/// </summary>
public abstract class CachedFeed : Feed
{
	protected CachedFeed(IItemStorage storage, Guid collectionIdentifier)
	{
		Storage = storage;
		CollectionIdentifier = collectionIdentifier;
	}
	
	/// <summary>
	/// Object which stores items.
	/// </summary>
	public IItemStorage Storage { get; }
	
	/// <summary>
	/// The item collection identifier.
	/// </summary>
	public Guid CollectionIdentifier { get; }

	protected sealed override async Task<IEnumerable<IReadOnlyStoredItem>> DoLoadAsync()
	{
		var items = await Storage.GetItemsAsync(CollectionIdentifier);
		_items = items.ToImmutableHashSet();
		return _items;
	}

	protected sealed override async Task<IEnumerable<IReadOnlyStoredItem>> DoSynchronizeAsync()
	{
		var fetchedItems = await DoFetchAsync();
		var newItems = await Storage.AddItemsAsync(fetchedItems, CollectionIdentifier);
		_items = _items.Union(newItems);
		return _items;
	}

	/// <summary>
	/// <para>Asynchronously fetch new or updated items from a remote location.</para>
	///
	/// <para>The cache implementation ensures that there is only one item for each URL. If a URL in the resulting list
	/// is already cached, its modified timestamp is checked and if this version is newer than the cached one, the
	/// cached item is updated.</para>
	/// </summary>
	protected abstract Task<IEnumerable<IReadOnlyItem>> DoFetchAsync();

	private ImmutableHashSet<IReadOnlyStoredItem> _items = ImmutableHashSet<IReadOnlyStoredItem>.Empty;
}
