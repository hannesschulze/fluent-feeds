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
public sealed class SyndicationUrlFeedFactory : IUrlFeedFactory
{
	public SyndicationUrlFeedFactory(IFeedStorage feedStorage)
	{
		_feedStorage = feedStorage;
	}
	
	public async Task<Feed> CreateAsync(Uri url)
	{
		var identifier = Guid.NewGuid();
		var downloader = new FeedDownloader(url);
		var itemStorage = _feedStorage.GetItemStorage(identifier);
		var feed = new SyndicationFeed(downloader, itemStorage, identifier, url);
		await feed.LoadMetadataAsync().ConfigureAwait(false);
		return feed;
	}

	private readonly IFeedStorage _feedStorage;
}
