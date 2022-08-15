using System;
using System.Threading.Tasks;
using FluentFeeds.App.Shared.Models.Database;

namespace FluentFeeds.App.Shared.Services.Default;

public class DatabaseService : IDatabaseService
{
	public Task<TResult> ExecuteAsync<TResult>(Func<AppDbContext, Task<TResult>> task) =>
		QueueTask(previous => Task.Run(
			async () =>
			{
				if (previous != null)
					await previous;
				await using var database = DoCreateContext();
				return await task(database);
			}));

	public Task ExecuteAsync(Func<AppDbContext, Task> task) =>
		QueueTask(previous => Task.Run(
			async () =>
			{
				if (previous != null)
					await previous;
				await using var database = DoCreateContext();
				await task(database);
			}));

	/// <summary>
	/// Add a new task to the queue which awaits the first parameter.
	/// </summary>
	private TInnerTask QueueTask<TInnerTask>(Func<Task?, TInnerTask> createTask) where TInnerTask : Task
	{
		_currentTask ??= Task.Run(DoInitializeAsync);
		var previousTask = !_currentTask.IsCompleted ? _currentTask : null;
		var nextTask = createTask(previousTask);
		_currentTask = nextTask;
		return nextTask;
	}

	/// <summary>
	/// Initialize the database. The service ensures that this is called before every other operation.
	/// </summary>
	protected virtual Task DoInitializeAsync() => throw new NotImplementedException();
	
	/// <summary>
	/// Create a new database context repository.
	/// </summary>
	protected virtual AppDbContext DoCreateContext() => throw new NotImplementedException();

	private Task? _currentTask;
}
