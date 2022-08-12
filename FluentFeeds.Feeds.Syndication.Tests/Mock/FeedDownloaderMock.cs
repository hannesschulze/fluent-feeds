using System.Threading.Tasks;
using FluentFeeds.Feeds.Syndication.Download;
using SysSyndicationFeed = System.ServiceModel.Syndication.SyndicationFeed;

namespace FluentFeeds.Feeds.Syndication.Tests.Mock;

public sealed class FeedDownloaderMock : IFeedDownloader
{
	public SysSyndicationFeed Feed { get; set; } = new();

	public Task<SysSyndicationFeed> DownloadAsync() => Task.FromResult(Feed);
}
