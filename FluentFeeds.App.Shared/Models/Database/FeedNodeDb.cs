using System;
using System.ComponentModel.DataAnnotations;
using FluentFeeds.Common;
using FluentFeeds.Feeds.Base.Nodes;

namespace FluentFeeds.App.Shared.Models.Database;

/// <summary>
/// Database representation of a <see cref="IReadOnlyFeedNode"/>.
/// </summary>
public class FeedNodeDb
{
	[Key]
	public Guid Identifier { get; set; }
	public FeedNodeDb? Parent { get; set; }
	public bool HasChildren { get; set; }
	public FeedNodeType Type { get; set; }
	public string? CustomSerialized { get; set; }
	public string? Title { get; set; }
	public Symbol? Symbol { get; set; }
	public bool IsUserCustomizable { get; set; }
}
