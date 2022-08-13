using System.Threading.Tasks;
using FluentFeeds.App.Shared.Models.Database;
using FluentFeeds.App.Shared.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace FluentFeeds.App.Shared.Tests.Services.Mock;

public sealed class DatabaseServiceMock : IDatabaseService
{
	public async Task InitializeAsync()
	{
		await _connection.OpenAsync();
		await using var context = CreateContext();
		await context.Database.EnsureCreatedAsync();
	}

	public AppDbContext CreateContext()
	{
		return new AppDbContext(new DbContextOptionsBuilder().UseSqlite(_connection).Options);
	}

	private readonly SqliteConnection _connection = new("Filename=:memory:");
}
