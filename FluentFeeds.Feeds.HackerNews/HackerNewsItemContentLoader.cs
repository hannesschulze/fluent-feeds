using System.Threading;
using System.Threading.Tasks;
using FluentFeeds.Feeds.Base.Items.Content;
using FluentFeeds.Feeds.HackerNews.Download;

namespace FluentFeeds.Feeds.HackerNews;

/// <summary>
/// Class responsible for loading the comments for a Hacker News item.
/// </summary>
public sealed class HackerNewsItemContentLoader : IItemContentLoader
{
	public HackerNewsItemContentLoader(IDownloader downloader, long identifier)
	{
		Downloader = downloader;
		Identifier = identifier;
	}
	
	/// <summary>
	/// Object used to download the item content.
	/// </summary>
	public IDownloader Downloader { get; }
	
	/// <summary>
	/// The item identifier.
	/// </summary>
	public long Identifier { get; }
	
	public Task<ItemContent> LoadAsync(bool reload = false, CancellationToken cancellation = default)
	{
		throw new System.NotImplementedException();
	}
}
