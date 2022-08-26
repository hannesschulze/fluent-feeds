using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace FluentFeeds.Feeds.HackerNews.Models;

/// <summary>
/// Response when fetching an item using the Algolia Hacker News API.
/// </summary>
public record ItemCommentsResponse([property:Required] [property:JsonPropertyName("id")] long Id)
{
	[JsonPropertyName("created_at")]
	public string? CreatedAt { get; init; }
	[JsonPropertyName("author")]
	public string? Author { get; init; }
	[JsonPropertyName("text")]
	public string? Text { get; init; }
	[JsonPropertyName("children")]
	public ImmutableArray<ItemCommentsResponse>? Children { get; init; } = ImmutableArray<ItemCommentsResponse>.Empty;
}
