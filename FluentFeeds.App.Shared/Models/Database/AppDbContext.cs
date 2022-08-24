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
	public DbSet<FeedDb> Feeds { get; set; } = null!;
	public DbSet<ItemDb> Items { get; set; } = null!;
	public DbSet<FeedItemDb> FeedItems { get; set; } = null!;
	public DbSet<DeletedItemDb> DeletedItems { get; set; } = null!;
}
