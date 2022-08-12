using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;
using SysSyndicationFeed = System.ServiceModel.Syndication.SyndicationFeed;

namespace FluentFeeds.Feeds.Syndication.Download;

/// <summary>
/// Default feed downloader.
/// </summary>
public sealed class FeedDownloader : IFeedDownloader
{
	public FeedDownloader(Uri url)
	{
		Url = url;
	}
	
	/// <summary>
	/// URL of the feed.
	/// </summary>
	public Uri Url { get; }
	
	public async Task<SysSyndicationFeed> DownloadAsync()
	{
		await using var stream = await _httpClient.GetStreamAsync(Url);
		using var xmlReader = XmlReader.Create(stream);
		return SysSyndicationFeed.Load(xmlReader);
	}

	private readonly HttpClient _httpClient = new();
}
