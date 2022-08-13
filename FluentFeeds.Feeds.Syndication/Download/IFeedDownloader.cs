using System.Threading.Tasks;
using SysSyndicationFeed = System.ServiceModel.Syndication.SyndicationFeed;

namespace FluentFeeds.Feeds.Syndication.Download;

/// <summary>
/// Abstraction for downloading syndication feeds.
/// </summary>
public interface IFeedDownloader
{
	Task<SysSyndicationFeed> DownloadAsync();
}
