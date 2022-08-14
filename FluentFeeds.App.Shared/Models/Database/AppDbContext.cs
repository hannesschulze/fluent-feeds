using System.Text.Json;
using FluentFeeds.Documents;
using Microsoft.EntityFrameworkCore;

namespace FluentFeeds.App.Shared.Models.Database;

/// <summary>
/// Database context for the app's data storage.
/// </summary>
public sealed class AppDbContext : DbContext
{
	public AppDbContext(DbContextOptions options) : base(options)
	{
	}

	public DbSet<FeedProviderDb> FeedProviders { get; set; } = null!;
	public DbSet<FeedNodeDb> FeedNodes { get; set; } = null!;
	public DbSet<ItemDb> Items { get; set; } = null!;
}
