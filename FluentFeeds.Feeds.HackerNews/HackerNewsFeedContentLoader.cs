using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentFeeds.Feeds.Base.Feeds.Content;
using FluentFeeds.Feeds.HackerNews.Download;
using FluentFeeds.Feeds.HackerNews.Helpers;
using FluentFeeds.Feeds.HackerNews.Models;

namespace FluentFeeds.Feeds.HackerNews;

/// <summary>
/// Class responsible for loading an overview for a Hacker News feed.
/// </summary>
public sealed class HackerNewsFeedContentLoader : IFeedContentLoader
{
	private const int MaxItemsPerFetch = 10;
	
	public HackerNewsFeedContentLoader(IDownloader downloader, HackerNewsFeedType feedType)
	{
		Downloader = downloader;
		FeedType = feedType;
	}
	
	/// <summary>
	/// Object used to download the feed content.
	/// </summary>
	public IDownloader Downloader { get; }
	
	/// <summary>
	/// The feed loaded by this object.
	/// </summary>
	public HackerNewsFeedType FeedType { get; }
	
	public async Task<FeedContent> LoadAsync()
	{
		var list = await Downloader.DownloadItemListAsync(FeedType);
		var items = new List<ItemResponse>();
		foreach (var identifier in list.Identifiers.Take(MaxItemsPerFetch))
		{
			items.Add(await Downloader.DownloadItemAsync(identifier));
		}

		var descriptors = await ConversionHelpers.ParallelTransformAsync(
			items, item => ConversionHelpers.ConvertItemDescriptorAsync(Downloader, item));
		return new FeedContent { Items = descriptors };
	}
}
