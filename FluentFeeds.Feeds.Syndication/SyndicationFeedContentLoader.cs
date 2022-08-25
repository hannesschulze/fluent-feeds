using System;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.ServiceModel.Syndication;
using System.Threading.Tasks;
using FluentFeeds.Feeds.Base.Feeds.Content;
using FluentFeeds.Feeds.Base.Items;
using FluentFeeds.Feeds.Syndication.Download;
using FluentFeeds.Feeds.Syndication.Helpers;

namespace FluentFeeds.Feeds.Syndication;

/// <summary>
/// Feed content loader implementation processing data from a <see cref="SyndicationFeed"/>.
/// </summary>
public sealed class SyndicationFeedContentLoader : IFeedContentLoader
{
	private const int ItemProcessingMaxDegreeOfParallelism = 3;
	
	public SyndicationFeedContentLoader(IFeedDownloader downloader, Uri url)
	{
		Downloader = downloader;
		Url = url;
	}

	/// <summary>
	/// Object used to download the feed.
	/// </summary>
	public IFeedDownloader Downloader { get; }

	/// <summary>
	/// URL of the feed.
	/// </summary>
	public Uri Url { get; }

	public async Task<FeedContent> LoadAsync()
	{
		var feed = await Downloader.DownloadAsync();
		var metadata = await ConversionHelpers.ConvertFeedMetadataAsync(feed, Url);
		var items = new ConcurrentBag<ItemDescriptor>();
		var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = ItemProcessingMaxDegreeOfParallelism };
		await Parallel.ForEachAsync(
			feed.Items, parallelOptions,
			async (item, _) => items.Add(await ConversionHelpers.ConvertItemAsync(item, Url)));
		return new FeedContent(metadata, items.ToArray());
	}
}
