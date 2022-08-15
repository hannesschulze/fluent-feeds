using System.Threading.Tasks;
using FluentFeeds.App.Shared.Models.Database;
using FluentFeeds.App.Shared.Services;
using FluentFeeds.App.Shared.Services.Default;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace FluentFeeds.App.Shared.Tests.Services.Mock;

public sealed class DatabaseServiceMock : DatabaseService
{
	public DatabaseServiceMock(ITestOutputHelper testOutputHelper)
	{
		_connection = new SqliteConnection("Filename=:memory:");
		_testOutputHelper = testOutputHelper;
	}

	protected override async Task DoInitializeAsync()
	{
		await _connection.OpenAsync();
		await using var context = DoCreateContext();
		await context.Database.EnsureCreatedAsync();
	}

	protected override AppDbContext DoCreateContext()
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
