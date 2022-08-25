using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace FluentFeeds.App.Shared.Models.Database;

/// <summary>
/// An item which was deleted by the user and should not appear again.
/// </summary>
[Index(nameof(ProviderIdentifier), nameof(StorageIdentifier))]
public class DeletedItemDb
{
	[Key]
	public Guid Identifier { get; set; }
	public Guid ProviderIdentifier { get; set; }
	public Guid StorageIdentifier { get; set; }
	public string UserIdentifier { get; set; } = String.Empty;
}
