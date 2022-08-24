using System;
using System.ComponentModel.DataAnnotations;

namespace FluentFeeds.App.Shared.Models.Database;

/// <summary>
/// An item which was deleted by the user and should not appear again.
/// </summary>
public class DeletedItemDb
{
	[Key]
	public Guid Identifier { get; set; }

	public string UserIdentifier { get; set; } = String.Empty;
}
