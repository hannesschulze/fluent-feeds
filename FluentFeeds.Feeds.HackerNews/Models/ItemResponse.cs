using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace FluentFeeds.Feeds.HackerNews.Models;

/// <summary>
/// Response when fetching an item from the official Hacker News API.
/// </summary>
public record ItemResponse(
	[property:Required] [property:JsonPropertyName("id")] long Id,
	[property:Required] [property:JsonPropertyName("type")] string Type)
{
	[JsonPropertyName("deleted")]
	public bool? Deleted { get; init; }
	[JsonPropertyName("dead")]
	public bool? Dead { get; init; }
	[JsonPropertyName("time")]
	public long? Time { get; init; }
	[JsonPropertyName("by")]
	public string? By { get; init; }
	[JsonPropertyName("text")]
	public string? Text { get; init; }
	[JsonPropertyName("url")]
	public string? Url { get; init; }
	[JsonPropertyName("title")]
	public string? Title { get; init; }
	[JsonPropertyName("kids")]
	public ImmutableArray<long>? Kids { get; init; }
}
