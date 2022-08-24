using System;
using System.ComponentModel.DataAnnotations;
using FluentFeeds.Common;
using FluentFeeds.Feeds.Base.Feeds;

namespace FluentFeeds.App.Shared.Models.Database;

/// <summary>
/// Database representation of a feed.
/// </summary>
public class FeedDb
{
	[Key]
	public Guid Identifier { get; set; }
	public FeedDb? Parent { get; set; }
	public bool HasChildren { get; set; }
	public FeedDescriptorType Type { get; set; }
	public string? Feed { get; set; }
	public string? Name { get; set; }
	public Symbol? Symbol { get; set; }
	public string? MetadataName { get; set; }
	public string? MetadataAuthor { get; set; }
	public string? MetadataDescription { get; set; }
	public Symbol? MetadataSymbol { get; set; }
	public Guid ItemStorageIdentifier { get; set; }
	public bool IsUserCustomizable { get; set; }
	public bool IsExcludedFromGroup { get; set; }
}
