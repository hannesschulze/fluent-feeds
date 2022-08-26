using System.Collections.Immutable;

namespace FluentFeeds.Feeds.HackerNews.Models;

/// <summary>
/// Response when listing items for a specific category using the official Hacker News API.
/// </summary>
/// <param name="Identifiers">The identifiers of the items in this category.</param>
public record ItemListResponse(ImmutableArray<long> Identifiers);
