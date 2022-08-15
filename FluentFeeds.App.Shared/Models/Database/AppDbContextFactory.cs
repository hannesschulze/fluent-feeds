using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace FluentFeeds.App.Shared.Models.Database;

/// <summary>
/// Used by the EF Core command line tools to generate migrations.
/// </summary>
public sealed class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
	public AppDbContext CreateDbContext(string[] args)
	{
		var options = new DbContextOptionsBuilder()
			.UseSqlite("Filename=:memory:")
			.Options;
		return new AppDbContext(options);
	}
}
