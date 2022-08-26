using System.Threading;
using System.Threading.Tasks;
using FluentFeeds.Feeds.HackerNews.Models;

namespace FluentFeeds.Feeds.HackerNews.Download;

/// <summary>
/// Abstraction for accessing the Hacker News API.
/// </summary>
public interface IDownloader
{
	/// <summary>
	/// Fetch the list of items for a feed.
	/// </summary>
	Task<ItemListResponse> DownloadItemListAsync(HackerNewsFeedType feedType, CancellationToken cancellation = default);

	/// <summary>
	/// Fetch an item with the provided identifier.
	/// </summary>
	Task<ItemResponse> DownloadItemAsync(long identifier, CancellationToken cancellation = default);
}
