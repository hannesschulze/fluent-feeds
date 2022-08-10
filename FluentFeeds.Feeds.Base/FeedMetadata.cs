using FluentFeeds.Common;

namespace FluentFeeds.Feeds.Base;

/// <summary>
/// Metadata for a <see cref="Feed"/>.
/// </summary>
/// <param name="Name">Name of the feed, used as the fallback title for a node.</param>
/// <param name="Author">Author of the feed's content.</param>
/// <param name="Description">Short description of the feed's content.</param>
/// <param name="Symbol">Symbol representing the feed.</param>
public record FeedMetadata(string? Name, string? Author, string? Description, Symbol? Symbol);
