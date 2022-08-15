using System;
using System.Threading.Tasks;
using FluentFeeds.App.Shared.Models.Database;

namespace FluentFeeds.App.Shared.Services;

/// <summary>
/// Service managing the local SQLite database.
/// </summary>
public interface IDatabaseService
{
	/// <summary>
	/// <para>Execute an operation which needs to access the database.</para>
	///
	/// <para>The task is executed on the thread pool. All database tasks are queued and only one task is processed at
	/// the same time.</para>
	/// </summary>
	Task ExecuteAsync(Func<AppDbContext, Task> task);

	/// <summary>
	/// <para>Execute an operation which needs to access the database.</para>
	///
	/// <para>The task is executed on the thread pool. All database tasks are queued and only one task is processed at
	/// the same time.</para>
	/// </summary>
	Task<TResult> ExecuteAsync<TResult>(Func<AppDbContext, Task<TResult>> task);
}
