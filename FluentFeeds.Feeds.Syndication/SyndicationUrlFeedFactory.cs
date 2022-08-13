using System;
using System.Threading.Tasks;
using FluentFeeds.Feeds.Base;
using FluentFeeds.Feeds.Base.Factories;
using FluentFeeds.Feeds.Base.Storage;
using FluentFeeds.Feeds.Syndication.Download;

namespace FluentFeeds.Feeds.Syndication;

/// <summary>
/// URL feed factory for creating syndication feeds.
/// </summary>
public class SyndicationUrlFeedFactory : IUrlFeedFactory
{
	public SyndicationUrlFeedFactory(IFeedStorage feedStorage)
	{
		FeedStorage = feedStorage;
	}
	
	public IFeedStorage FeedStorage { get; }
	
	public async Task<Feed> CreateAsync(Uri url)
	{
		var identifier = Guid.NewGuid();
		var downloader = CreateDownloader(url);
		var itemStorage = FeedStorage.GetItemStorage(identifier);
		var feed = new SyndicationFeed(downloader, itemStorage, identifier, url);
		await feed.LoadMetadataAsync().ConfigureAwait(false);
		return feed;
	}

	protected virtual IFeedDownloader CreateDownloader(Uri url) => new FeedDownloader(url);
}
