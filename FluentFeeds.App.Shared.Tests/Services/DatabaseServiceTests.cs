using System.Threading.Tasks;
using FluentFeeds.App.Shared.Tests.Services.Mock;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Xunit.Abstractions;

namespace FluentFeeds.App.Shared.Tests.Services;

public class DatabaseServiceTests
{
	public DatabaseServiceTests(ITestOutputHelper testOutputHelper)
	{
		_testOutputHelper = testOutputHelper;
	}
	
	[Fact]
	public async Task InitializesDatabaseBeforeOperations()
	{
		var service = new DatabaseServiceMock(_testOutputHelper);
		await service.ExecuteAsync(
			async database =>
			{
				Assert.Empty(await database.FeedProviders.ToListAsync());
				Assert.Empty(await database.FeedNodes.ToListAsync());
				Assert.Empty(await database.Items.ToListAsync());
			});
	}

	[Fact]
	public async Task EnsuresCompletionBeforeStartingTask_WithReturnType()
	{
		var service = new DatabaseServiceMock(_testOutputHelper);
		var completionSourceA = new TaskCompletionSource<int>();
		var completionSourceB = new TaskCompletionSource<int>();
		var taskA = service.ExecuteAsync(_ => completionSourceA.Task);
		var taskB = service.ExecuteAsync(_ => completionSourceB.Task);
		Assert.False(taskA.IsCompleted);
		Assert.False(taskB.IsCompleted);
		completionSourceB.SetResult(0);
		Assert.False(taskA.IsCompleted);
		Assert.False(taskB.IsCompleted);
		completionSourceA.SetResult(1);
		Assert.Equal(1, await taskA);
		Assert.Equal(0, await taskB);
	}

	[Fact]
	public async Task EnsuresCompletionBeforeStartingTask_WithoutReturnType()
	{
		var service = new DatabaseServiceMock(_testOutputHelper);
		var completionSourceA = new TaskCompletionSource();
		var completionSourceB = new TaskCompletionSource();
		var taskA = service.ExecuteAsync(_ => completionSourceA.Task);
		var taskB = service.ExecuteAsync(_ => completionSourceB.Task);
		Assert.False(taskA.IsCompleted);
		Assert.False(taskB.IsCompleted);
		completionSourceB.SetResult();
		Assert.False(taskA.IsCompleted);
		Assert.False(taskB.IsCompleted);
		completionSourceA.SetResult();
		await taskA;
		await taskB;
	}

	[Fact]
	public async Task ExecuteTaskWithoutInProgressTask_WithReturnType()
	{
		var service = new DatabaseServiceMock(_testOutputHelper);
		await service.ExecuteAsync(_ => Task.CompletedTask);
		var completionSource = new TaskCompletionSource<int>();
		var task = service.ExecuteAsync(_ => completionSource.Task);
		Assert.False(task.IsCompleted);
		completionSource.SetResult(1);
		Assert.Equal(1, await task);
	}

	[Fact]
	public async Task ExecuteTaskWithoutInProgressTask_WithoutReturnType()
	{
		var service = new DatabaseServiceMock(_testOutputHelper);
		await service.ExecuteAsync(_ => Task.CompletedTask);
		var completionSource = new TaskCompletionSource();
		var task = service.ExecuteAsync(_ => completionSource.Task);
		Assert.False(task.IsCompleted);
		completionSource.SetResult();
		await task;
	}

	private readonly ITestOutputHelper _testOutputHelper;
}

