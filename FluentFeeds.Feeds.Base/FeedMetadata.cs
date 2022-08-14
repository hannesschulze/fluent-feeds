using FluentFeeds.Common;

namespace FluentFeeds.Feeds.Base;

/// <summary>
/// Metadata for a <see cref="Feed"/>.
/// </summary>
public record FeedMetadata
{
	/// <summary>
	/// Name of the feed, used as the fallback title for a node.
	/// </summary>
	public string? Name { get; init; }
	
	/// <summary>
	/// Author of the feed's content.
	/// </summary>
	public string? Author { get; init; }
	
	/// <summary>
	/// Short description of the feed's content.
	/// </summary>
	public string? Description { get; init; }
	
	/// <summary>
	/// Symbol representing the feed.
	/// </summary>
	public Symbol? Symbol { get; init; }
}
