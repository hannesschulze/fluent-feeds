using System;
using System.Net.Http;
using System.ServiceModel.Syndication;
using System.Threading.Tasks;
using System.Xml;

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
	
	public async Task<SyndicationFeed> DownloadAsync()
	{
		await using var stream = await _httpClient.GetStreamAsync(Url);
		using var xmlReader = XmlReader.Create(stream);
		return SyndicationFeed.Load(xmlReader);
	}

	private readonly HttpClient _httpClient = new();
}
