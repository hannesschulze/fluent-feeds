using System;
using System.ComponentModel.DataAnnotations;
using FluentFeeds.Feeds.Base;

namespace FluentFeeds.App.Shared.Models.Database;

/// <summary>
/// Database object describing a previously loaded feed provider.
/// </summary>
public class FeedProviderDb
{
	[Key]
	public Guid Identifier { get; set; }
	public FeedNodeDb RootNode { get; set; } = null!;
}
