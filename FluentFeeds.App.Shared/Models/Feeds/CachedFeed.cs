using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using FluentFeeds.Feeds.Base.EventArgs;
using FluentFeeds.Feeds.Base.Feeds.Content;
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
		await UpdateItemsAsync();
		Storage.ItemsDeleted += HandleItemsDeleted;
	}

	protected sealed override async Task DoSynchronizeAsync()
	{
		var fetchResult = await Task.Run(DoFetchAsync);
		var newItems = await Storage.AddItemsAsync(fetchResult.Items);
		Items = Items.Union(newItems);
		if (fetchResult.UpdatedMetadata != null && fetchResult.UpdatedMetadata != Metadata)
		{
			Metadata = fetchResult.UpdatedMetadata;
		}
	}

	private async void HandleItemsDeleted(object? sender, ItemsDeletedEventArgs e)
	{
		await UpdateItemsAsync();
	}

	private async ValueTask UpdateItemsAsync()
	{
		var items = await Storage.GetItemsAsync();
		Items = items.ToImmutableHashSet();
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
