using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentFeeds.Feeds.Base.Items;
using FluentFeeds.Feeds.Base.Storage;

namespace FluentFeeds.Feeds.Base;

/// <summary>
/// A feed subclasses which loads and saves items into a <see cref="IItemStorage"/>.
/// </summary>
public abstract class CachedFeed : Feed
{
	protected CachedFeed(IItemStorage storage)
	{
		Storage = storage;
	}
	
	/// <summary>
	/// Object which stores items.
	/// </summary>
	public IItemStorage Storage { get; }

	protected sealed override async Task<IEnumerable<IReadOnlyStoredItem>> DoLoadAsync()
	{
		foreach (var item in await Storage.GetItemsAsync())
		{
			_cache.Add(item.Url, item);
		}

		return _cache.Values;
	}

	protected sealed override async Task<IEnumerable<IReadOnlyStoredItem>> DoSynchronizeAsync()
	{
		var added = new List<IReadOnlyItem>();
		
		foreach (var item in await DoFetchAsync())
		{
			if (_cache.TryGetValue(item.Url, out var existing))
			{
				// There is already an item associated with this URL, determine if it was modified.
				if (item.ModifiedTimestamp > existing.ModifiedTimestamp)
				{
					// If it was modified, save the updated item.
					await Storage.UpdateItemAsync(existing.Identifier, item);
				}
			}
			else
			{
				// This is a new item.
				added.Add(item);
			}
		}

		foreach (var item in await Storage.AddItemsAsync(added))
		{
			_cache.Add(item.Url, item);
		}

		return _cache.Values;
	}
	
	/// <summary>
	/// <para>Check if an item with the specified URL is either new or was updated if its modified timestamp is the one
	/// provided.</para>
	///
	/// <para>This can be useful when implementing <see cref="DoFetchAsync"/> to avoid the additional overhead of
	/// loading already up-to-date items.</para>
	/// </summary>
	protected bool IsNewOrUpdated(Uri url, DateTimeOffset modifiedTimestamp) =>
		!_cache.TryGetValue(url, out var existing) || modifiedTimestamp > existing.ModifiedTimestamp;

	/// <summary>
	/// <para>Asynchronously fetch new or updated items from a remote location.</para>
	///
	/// <para>The cache implementation ensures that there is only one item for each URL. If a URL in the resulting list
	/// is already cached, its modified timestamp is checked and if this version is newer than the cached one, the
	/// cached item is updated.</para>
	/// </summary>
	/// <remarks>
	/// After returning from this method, all items returned are owned by the cache and should not be modified directly
	/// anymore.
	/// </remarks>
	protected abstract Task<IEnumerable<IReadOnlyItem>> DoFetchAsync();

	private readonly Dictionary<Uri, IReadOnlyStoredItem> _cache = new();
}
