using System;
using System.ComponentModel.DataAnnotations;
using FluentFeeds.Common;
using FluentFeeds.Feeds.Base.Feeds;
using Microsoft.EntityFrameworkCore;

namespace FluentFeeds.App.Shared.Models.Database;

/// <summary>
/// Database representation of a feed.
/// </summary>
[Index(nameof(ProviderIdentifier), nameof(ItemStorageIdentifier))]
public class FeedDb
{
	[Key]
	public Guid Identifier { get; set; }
	public Guid ProviderIdentifier { get; set; }
	public Guid ItemStorageIdentifier { get; set; }
	public FeedDb? Parent { get; set; }
	public bool HasChildren { get; set; }
	public FeedDescriptorType Type { get; set; }
	public string? ContentLoader { get; set; }
	public string? Name { get; set; }
	public Symbol? Symbol { get; set; }
	public string? MetadataName { get; set; }
	public string? MetadataAuthor { get; set; }
	public string? MetadataDescription { get; set; }
	public Symbol? MetadataSymbol { get; set; }
	public bool IsUserCustomizable { get; set; }
	public bool IsExcludedFromGroup { get; set; }
}
