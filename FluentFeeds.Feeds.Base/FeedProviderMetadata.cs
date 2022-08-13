using System;

namespace FluentFeeds.Feeds.Base;

/// <summary>
/// Metadata for a <see cref="FeedProvider"/>.
/// </summary>
/// <param name="Identifier">
/// Unique identifier for the feed provider. This should be consistent every time the provider is created.
/// </param>
/// <param name="Name">Human-readable name of the feed provider.</param>
/// <param name="Description">Human-readable description of the feed provider.</param>
public record FeedProviderMetadata(Guid Identifier, string Name, string Description);
