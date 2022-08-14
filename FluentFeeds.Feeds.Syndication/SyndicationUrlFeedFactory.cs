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
	public async Task<Feed> CreateAsync(IFeedStorage feedStorage, Uri url)
	{
		var identifier = Guid.NewGuid();
		var downloader = CreateDownloader(url);
		var itemStorage = feedStorage.GetItemStorage(identifier);
		var feed = new SyndicationFeed(downloader, itemStorage, identifier, url);
		await feed.LoadMetadataAsync();
		return feed;
	}

	protected virtual IFeedDownloader CreateDownloader(Uri url) => new FeedDownloader(url);
}
