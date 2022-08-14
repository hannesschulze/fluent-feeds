using System;
using System.Linq;
using System.Threading.Tasks;
using FluentFeeds.Feeds.Base.Tests.Mock;
using Xunit;

namespace FluentFeeds.Feeds.Base.Tests;

public class CachedFeedTests
{
	[Fact]
	public async Task LoadItems()
	{
		var storage = new ItemStorageMock();
		_ = storage.AddItems(new[] { TestHelpers.CreateItem(Guid.Empty), TestHelpers.CreateItem(Guid.Empty) }).ToList();
		var feed = new CachedFeedMock(storage);
		await feed.LoadAsync();
		Assert.Equal(2, feed.Items.Count);
	}

	[Fact]
	public async Task SynchronizeItems()
	{
		var storage = new ItemStorageMock();
		var feed = new CachedFeedMock(storage);
		_ = feed.LoadAsync();
		var taskA = feed.SynchronizeAsync();
		Assert.False(taskA.IsCompleted);
		feed.CompleteFetch(TestHelpers.CreateItem(Guid.NewGuid()));
		await taskA;
		Assert.Single(feed.Items);
		var taskB = feed.SynchronizeAsync();
		Assert.False(taskB.IsCompleted);
		feed.CompleteFetch(TestHelpers.CreateItem(Guid.NewGuid()));
		await taskB;
		Assert.Equal(2, feed.Items.Count);
	}
}
