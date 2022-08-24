using System.Threading.Tasks;

namespace FluentFeeds.App.Shared.Models.Feeds.Loaders;

/// <summary>
/// A feed loading implementation not containing any items.
/// </summary>
public sealed class EmptyFeedLoader : FeedLoader
{
	protected override Task DoInitializeAsync() => Task.CompletedTask;

	protected override Task DoSynchronizeAsync() => Task.CompletedTask;
}
