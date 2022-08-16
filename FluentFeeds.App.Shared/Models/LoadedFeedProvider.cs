using FluentFeeds.Feeds.Base;
using FluentFeeds.Feeds.Base.Nodes;
using FluentFeeds.Feeds.Base.Storage;

namespace FluentFeeds.App.Shared.Models;

/// <summary>
/// Representation of a <see cref="FeedProvider"/> which has been loaded and properly initialized.
/// </summary>
/// <param name="Provider">The feed provider instance.</param>
/// <param name="RootNode">Root node for this feed provider in the node tree.</param>
/// <param name="FeedStorage">The storage object designated for this feed provider.</param>
public record LoadedFeedProvider(FeedProvider Provider, IReadOnlyFeedNode RootNode, IFeedStorage FeedStorage);
