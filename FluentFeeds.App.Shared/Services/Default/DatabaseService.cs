using System;
using System.IO;
using System.Threading.Tasks;
using FluentFeeds.App.Shared.Models.Database;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace FluentFeeds.App.Shared.Services.Default;

public class DatabaseService : IDatabaseService
{
	private const string AppDataFolderName = "FluentFeeds";

	public DatabaseService()
	{
		_initialize = new Lazy<Task>(InitializeAsync, isThreadSafe: true);
	}
	
	public Task<TResult> ExecuteAsync<TResult>(Func<AppDbContext, Task<TResult>> task) =>
		QueueTask(previous => Task.Run(
			async () =>
			{
				if (previous != null)
				{
					try
					{
						await previous;
					}
					catch (Exception)
					{
						// ignored
					}
				}
				await _initialize.Value;
				await using var database = CreateContext();
				return await task(database);
			}));

	public Task ExecuteAsync(Func<AppDbContext, Task> task) =>
		QueueTask(previous => Task.Run(
			async () =>
			{
				if (previous != null)
				{
					try
					{
						await previous;
					}
					catch (Exception)
					{
						// ignored
					}
				}
				await _initialize.Value;
				await using var database = CreateContext();
				await task(database);
			}));

	/// <summary>
	/// Add a new task to the queue which awaits the first parameter.
	/// </summary>
	private TInnerTask QueueTask<TInnerTask>(Func<Task?, TInnerTask> createTask) where TInnerTask : Task
	{
		var previousTask = !_currentTask.IsCompleted ? _currentTask : null;
		var nextTask = createTask(previousTask);
		_currentTask = nextTask;
		return nextTask;
	}

	/// <summary>
	/// Create a database connection.
	/// </summary>
	protected virtual SqliteConnection CreateConnection()
	{
		var allAppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
		var appDataPath = Path.Combine(allAppDataPath, AppDataFolderName);
		Directory.CreateDirectory(appDataPath);
		var dbPath = Path.Combine(appDataPath, "db.sqlite3");
		return new SqliteConnection($"Filename={dbPath}");
	}

	/// <summary>
	/// Manually adjust the db context options.
	/// </summary>
	protected virtual void ConfigureContext(DbContextOptionsBuilder options)
	{
	}

	/// <summary>
	/// Initialize the database. The service ensures that this is called before every other operation.
	/// </summary>
	private async Task InitializeAsync()
	{
		_connection = CreateConnection();
		await _connection.OpenAsync();
		await using var context = CreateContext();
		await context.Database.MigrateAsync();
	}
	
	/// <summary>
	/// Create a new database context repository.
	/// </summary>
	private AppDbContext CreateContext()
	{
		var optionsBuilder = new DbContextOptionsBuilder();
		optionsBuilder.UseSqlite(_connection!);
		ConfigureContext(optionsBuilder);
		return new AppDbContext(optionsBuilder.Options);
	}

	private SqliteConnection? _connection;
	private Lazy<Task> _initialize;
	private Task _currentTask = Task.CompletedTask;
}
