using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentFeeds.Feeds.Base;
using FluentFeeds.Feeds.Base.Items;
using FluentFeeds.Feeds.Base.Storage;
using FluentFeeds.Feeds.Syndication.Download;
using FluentFeeds.Feeds.Syndication.Helpers;
using SysSyndicationFeed = System.ServiceModel.Syndication.SyndicationFeed;

namespace FluentFeeds.Feeds.Syndication;

/// <summary>
/// Feed implementation processing data from a <see cref="System.ServiceModel.Syndication.SyndicationFeed"/>.
/// </summary>
public sealed class SyndicationFeed : CachedFeed
{
	private const int ItemProcessingMaxDegreeOfParallelism = 3;
	
	public SyndicationFeed(
		IFeedDownloader downloader, IItemStorage storage, Guid identifier, Uri url, 
		FeedMetadata? initialMetadata = null) : base(storage)
	{
		Downloader = downloader;
		Identifier = identifier;
		Url = url;
		Metadata = initialMetadata;
	}

	/// <summary>
	/// Object used to download the feed.
	/// </summary>
	public IFeedDownloader Downloader { get; }
	
	/// <summary>
	/// Unique identifier for the feed.
	/// </summary>
	public Guid Identifier { get; }
	
	/// <summary>
	/// URL of the feed.
	/// </summary>
	public Uri Url { get; }

	/// <summary>
	/// Pre-fetch the feed ahead of time and populate its metadata.
	/// </summary>
	public async Task LoadMetadataAsync()
	{
		Metadata = await Task.Run(
			async () =>
			{
				var feed = await Downloader.DownloadAsync();
				lock (this)
				{
					_prefetchedFeed = feed;
				}
				return await ConversionHelpers.ConvertFeedMetadataAsync(feed, Url);
			});
	}

	protected override async Task<FetchResult> DoFetchAsync()
	{
		SysSyndicationFeed? feed;
		lock (this)
		{
			feed = _prefetchedFeed;
			_prefetchedFeed = null;
		}
	
		FeedMetadata? updatedMetadata = null;	
		if (feed == null)
		{
			feed = await Downloader.DownloadAsync();
			updatedMetadata = await ConversionHelpers.ConvertFeedMetadataAsync(feed, Url);
		}

		// Process all items in parallel.
		var result = new ConcurrentBag<IReadOnlyItem>();
		var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = ItemProcessingMaxDegreeOfParallelism };
		await Parallel.ForEachAsync(
			feed.Items, parallelOptions,
			async (item, _) => result.Add(await ConversionHelpers.ConvertItemAsync(item, Url)));
		return new FetchResult(result.ToArray()) { UpdatedMetadata = updatedMetadata };
	}

	private SysSyndicationFeed? _prefetchedFeed;
}
