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
		_testOutputHelper = testOutputHelper;
	}

	protected override SqliteConnection CreateConnection()
	{
		return new SqliteConnection("Filename=:memory:");
	}

	protected override void ConfigureContext(DbContextOptionsBuilder options)
	{
		base.ConfigureContext(options);
		options.LogTo(_testOutputHelper.WriteLine, LogLevel.Information);
	}

	private readonly ITestOutputHelper _testOutputHelper;
}
