using System.Collections.Immutable;

namespace FluentFeeds.Feeds.HackerNews.Models;

/// <summary>
/// Response when fetching an item.
/// </summary>
public record ItemResponse(long Id, string Type)
{
	public bool? Deleted { get; init; }
	public bool? Dead { get; init; }
	public long? Time { get; init; }
	public long? Descendants { get; init; }
	public long? Score { get; init; }
	public long? Parent { get; init; } 
	public ImmutableArray<long>? Kids { get; init; }
	public ImmutableArray<long>? Parts { get; init; }
	public string? By { get; init; }
	public string? Text { get; init; }
	public string? Url { get; init; }
	public string? Title { get; init; }
}
