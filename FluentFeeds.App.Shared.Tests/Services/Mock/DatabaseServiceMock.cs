using System.Threading.Tasks;
using FluentFeeds.App.Shared.Models.Database;
using FluentFeeds.App.Shared.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace FluentFeeds.App.Shared.Tests.Services.Mock;

public sealed class DatabaseServiceMock : IDatabaseService
{
	public DatabaseServiceMock(ITestOutputHelper testOutputHelper)
	{
		_connection = new SqliteConnection("Filename=:memory:");
		_testOutputHelper = testOutputHelper;
	}
	
	public async Task InitializeAsync()
	{
		await _connection.OpenAsync();
		await using var context = CreateContext();
		await context.Database.EnsureCreatedAsync();
	}

	public AppDbContext CreateContext()
	{
		var options = new DbContextOptionsBuilder()
			.UseSqlite(_connection)
			.LogTo(_testOutputHelper.WriteLine, LogLevel.Information)
			.Options;
		return new AppDbContext(options);
	}

	private readonly SqliteConnection _connection;
	private readonly ITestOutputHelper _testOutputHelper;
}
