using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace FluentFeeds.App.Shared.Models.Database;

/// <summary>
/// Database relationship between a feed and an item.
/// </summary>
[Index(nameof(FeedIdentifier))]
public class FeedItemDb
{
	[Key]
	public Guid Identifier { get; set; }
	public Guid FeedIdentifier { get; set; }
	public Guid ItemIdentifier { get; set; }
}
