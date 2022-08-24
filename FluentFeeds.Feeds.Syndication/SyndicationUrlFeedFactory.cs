using System;
using FluentFeeds.Feeds.Base.Factories;
using FluentFeeds.Feeds.Base.Feeds.Content;
using FluentFeeds.Feeds.Syndication.Download;

namespace FluentFeeds.Feeds.Syndication;

/// <summary>
/// URL feed factory for creating syndication feeds.
/// </summary>
public class SyndicationUrlFeedFactory : IUrlFeedFactory
{
	public IFeedContentLoader Create(Uri url)
	{
		var downloader = CreateDownloader(url);
		return new SyndicationFeedContentLoader(downloader, url);
	}

	protected virtual IFeedDownloader CreateDownloader(Uri url) => new FeedDownloader(url);
}
