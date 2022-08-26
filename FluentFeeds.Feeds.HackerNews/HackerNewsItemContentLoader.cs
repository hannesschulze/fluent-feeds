using System.Threading;
using System.Threading.Tasks;
using FluentFeeds.Feeds.Base.Items.Content;

namespace FluentFeeds.Feeds.HackerNews;

/// <summary>
/// Class responsible for loading the comments for a Hacker News item.
/// </summary>
public sealed class HackerNewsItemContentLoader : IItemContentLoader
{
	public Task<ItemContent> LoadAsync(bool reload = false, CancellationToken cancellation = default)
	{
		throw new System.NotImplementedException();
	}
}
