using System;
using System.Collections.Generic;
using System.Linq;
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
	public SyndicationFeed(
		IFeedDownloader downloader, IItemStorage storage, Guid identifier, Uri url, 
		FeedMetadata? initialMetadata = null)
		: base(storage, identifier)
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
		var feed = _prefetchedFeed = await Downloader.DownloadAsync();
		Metadata = await ConversionHelpers.ConvertFeedMetadataAsync(feed);
	}

	protected override async Task<IEnumerable<IReadOnlyItem>> DoFetchAsync()
	{
		SysSyndicationFeed feed;
		if (_prefetchedFeed != null)
		{
			feed = _prefetchedFeed;
			_prefetchedFeed = null;
		}
		else
		{
			feed = await Downloader.DownloadAsync();
			Metadata = await ConversionHelpers.ConvertFeedMetadataAsync(feed);
		}

		// Process all items in parallel.
		return await Task.WhenAll(feed.Items.Select(ConversionHelpers.ConvertItemAsync));
	}

	private SysSyndicationFeed? _prefetchedFeed;
}
