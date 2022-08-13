using System;
using FluentFeeds.Feeds.Base;
using FluentFeeds.Feeds.Base.Nodes;

namespace FluentFeeds.App.Shared.Models;

/// <summary>
/// Representation of a <see cref="FeedProvider"/> which has been loaded and properly initialized.
/// </summary>
/// <param name="Identifier">Identifier for the provider in the local database.</param>
/// <param name="RootNode">Root node for this feed provider in the node tree.</param>
/// <param name="Provider">The feed provider instance.</param>
public record LoadedFeedProvider(Guid Identifier, IReadOnlyFeedNode RootNode, FeedProvider Provider);
