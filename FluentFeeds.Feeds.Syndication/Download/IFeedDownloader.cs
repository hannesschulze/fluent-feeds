using System.ServiceModel.Syndication;
using System.Threading.Tasks;

namespace FluentFeeds.Feeds.Syndication.Download;

/// <summary>
/// Abstraction for downloading syndication feeds.
/// </summary>
public interface IFeedDownloader
{
	Task<SyndicationFeed> DownloadAsync();
}
