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
	/// <summary>
	/// Result of a fetch request.
	/// </summary>
	public record FetchResult(IEnumerable<IReadOnlyItem> Items)
	{
		/// <summary>
		/// Updated metadata (if the metadata has changed).
		/// </summary>
		public FeedMetadata? UpdatedMetadata { get; init; }
	}
	
	protected CachedFeed(IItemStorage storage)
	{
		Storage = storage;
	}
	
	/// <summary>
	/// Object which stores items.
	/// </summary>
	public IItemStorage Storage { get; }

	protected sealed override async Task DoLoadAsync()
	{
		var items = await Storage.GetItemsAsync();
		Items = items.ToImmutableHashSet();
	}

	protected sealed override async Task DoSynchronizeAsync()
	{
		var (items, metadata) = await Task.Run(
			async () =>
			{
				var fetchResult = await DoFetchAsync();
				var newItems = await Storage.AddItemsAsync(fetchResult.Items);
				return (newItems, fetchResult.UpdatedMetadata);
			});
		Items = Items.Union(items);
		if (metadata != null && metadata != Metadata)
		{
			Metadata = metadata;
		}
	}

	/// <summary>
	/// <para>Asynchronously fetch new or updated items from a remote location.</para>
	///
	/// <para>The cache implementation ensures that there is only one item for each URL. If a URL in the resulting list
	/// is already cached, its modified timestamp is checked and if this version is newer than the cached one, the
	/// cached item is updated.</para>
	/// </summary>
	/// <remarks>
	/// This method is called on the thread pool. The implementation should not update properties like metadata or
	/// items.
	/// </remarks>
	protected abstract Task<FetchResult> DoFetchAsync();
}
