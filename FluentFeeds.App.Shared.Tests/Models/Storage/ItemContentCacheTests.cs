using System;
using System.Threading.Tasks;
using FluentFeeds.App.Shared.Models.Storage;
using FluentFeeds.App.Shared.Tests.Mock;
using FluentFeeds.Feeds.Base.Items.Content;
using Xunit;

namespace FluentFeeds.App.Shared.Tests.Models.Storage;

public class ItemContentCacheTests
{
	[Fact]
	public void NewIdentifier()
	{
		var cache = new ItemContentCache(2);
		var completionSource = new TaskCompletionSource<IItemContentLoader>();
		var loader = new ItemContentLoaderMock();
		var loadCalled = false;
		var task = cache.GetLoaderAsync(
			Guid.Empty,
			() =>
			{
				loadCalled = true;
				return completionSource.Task;
			});
		Assert.False(task.IsCompleted);
		Assert.True(loadCalled);
		completionSource.SetResult(loader);
		Assert.True(task.IsCompleted);
		Assert.Equal(loader, task.Result);
	}

	[Fact]
	public async Task LoadingAlreadyInProgress()
	{
		var cache = new ItemContentCache(2);
		var completionSource = new TaskCompletionSource<IItemContentLoader>();
		var loader = new ItemContentLoaderMock();
		var taskA = cache.GetLoaderAsync(Guid.Empty, () => completionSource.Task);
		var taskB = cache.GetLoaderAsync(Guid.Empty, () => throw new Exception());
		Assert.False(taskA.IsCompleted);
		Assert.False(taskB.IsCompleted);
		completionSource.SetResult(loader);
		await taskA;
		await taskB;
		Assert.Equal(loader, taskA.Result);
		Assert.Equal(loader, taskB.Result);
	}

	[Fact]
	public void LoadingFails()
	{
		var cache = new ItemContentCache(2);
		var taskA = cache.GetLoaderAsync(Guid.Empty, () => throw new Exception());
		Assert.True(taskA.IsCompleted);
		Assert.ThrowsAny<Exception>(() => taskA.Result);
		var loader = new ItemContentLoaderMock();
		var taskB = cache.GetLoaderAsync(Guid.Empty, () => Task.FromResult<IItemContentLoader>(loader));
		Assert.True(taskB.IsCompleted);
		Assert.Equal(loader, taskB.Result);
	}

	[Fact]
	public async Task Caching()
	{
		var cache = new ItemContentCache(2);
		var identifierA = Guid.Parse("9267f74e-0b23-4172-b7fa-524927a84584");
		var identifierB = Guid.Parse("dd6c322b-c7c9-4011-8374-04da25d0db84");
		var identifierC = Guid.Parse("afb794af-a632-47db-96d5-c3907e0cf642");
		var loaderA = new ItemContentLoaderMock();
		var resultA = await cache.GetLoaderAsync(identifierA, () => Task.FromResult<IItemContentLoader>(loaderA));
		Assert.Equal(loaderA, resultA);
		var loaderB = new ItemContentLoaderMock();
		var resultB = await cache.GetLoaderAsync(identifierB, () => Task.FromResult<IItemContentLoader>(loaderB));
		Assert.Equal(loaderB, resultB);
		var loaderC = new ItemContentLoaderMock();
		var resultC = await cache.GetLoaderAsync(identifierA, () => Task.FromResult<IItemContentLoader>(loaderC));
		Assert.Equal(loaderA, resultC);
		var loaderD = new ItemContentLoaderMock();
		var resultD = await cache.GetLoaderAsync(identifierC, () => Task.FromResult<IItemContentLoader>(loaderD));
		Assert.Equal(loaderD, resultD);
		var loaderE = new ItemContentLoaderMock();
		var resultE = await cache.GetLoaderAsync(identifierA, () => Task.FromResult<IItemContentLoader>(loaderE));
		Assert.Equal(loaderA, resultE);
		var loaderF = new ItemContentLoaderMock();
		var resultF = await cache.GetLoaderAsync(identifierB, () => Task.FromResult<IItemContentLoader>(loaderF));
		Assert.Equal(loaderF, resultF);
	}
}
