using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace FluentFeeds.App.Shared.Models.Database;

/// <summary>
/// Database representation of an item.
/// </summary>
[Index(nameof(ProviderIdentifier), nameof(StorageIdentifier))]
public class ItemDb
{
	[Key]
	public Guid Identifier { get; set; }
	public Guid ProviderIdentifier { get; set; }
	public Guid StorageIdentifier { get; set; }
	public Uri? Url { get; set; }
	public Uri? ContentUrl { get; set; }
	public DateTimeOffset PublishedTimestamp { get; set; }
	public DateTimeOffset ModifiedTimestamp { get; set; }
	public string Title { get; set; } = String.Empty;
	public string? Author { get; set; }
	public string? Summary { get; set; }
	public string Content { get; set; } = String.Empty;
	public bool IsRead { get; set; }
}
