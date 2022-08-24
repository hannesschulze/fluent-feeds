using System.Collections.Immutable;
using System.Threading.Tasks;
using FluentFeeds.App.Shared.EventArgs;
using FluentFeeds.App.Shared.Models.Storage;
using FluentFeeds.Feeds.Base.Feeds.Content;

namespace FluentFeeds.App.Shared.Models.Feeds.Loaders;

/// <summary>
/// A feed loader subclass which loads and saves items into an <see cref="IItemStorage"/>.
/// </summary>
public abstract class CachedFeedLoader : FeedLoader
{
	protected CachedFeedLoader(IFeedView feed, IItemStorage storage, IFeedContentLoader contentLoader) : base(feed)
	{
		Storage = storage;
		ContentLoader = contentLoader;
	}
	
	/// <summary>
	/// Object which stores items.
	/// </summary>
	public IItemStorage Storage { get; }
	
	/// <summary>
	/// Content loader object fetching new data.
	/// </summary>
	public IFeedContentLoader ContentLoader { get; }

	protected sealed override async Task DoInitializeAsync()
	{
		await ReloadItemsAsync();
		Storage.ItemsDeleted += HandleItemsDeleted;
	}

	protected sealed override async Task DoSynchronizeAsync()
	{
		var content = await Task.Run(ContentLoader.LoadAsync);
		var newItems = await Storage.AddItemsAsync(content.Items, Feed.Identifier);
		Items = Items.Union(newItems);
		if (content.Metadata != Feed.Metadata && Feed.Storage != null)
		{
			await Feed.Storage.UpdateFeedMetadataAsync(Feed.Identifier, content.Metadata);
		}
	}

	private async void HandleItemsDeleted(object? sender, ItemsDeletedEventArgs e)
	{
		await ReloadItemsAsync();
	}

	private async ValueTask ReloadItemsAsync()
	{
		var items = await Storage.GetItemsAsync(Feed.Identifier);
		Items = items.ToImmutableHashSet();
	}
}
