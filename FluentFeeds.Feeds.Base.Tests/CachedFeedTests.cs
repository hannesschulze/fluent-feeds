using System;
using System.Linq;
using FluentFeeds.Feeds.Base.Tests.Mock;
using Xunit;

namespace FluentFeeds.Feeds.Base.Tests;

public class CachedFeedTests
{
	[Fact]
	public void LoadItems()
	{
		var storage = new ItemStorageMock();
		_ = storage.AddItems(new[] { TestHelpers.CreateItem(Guid.Empty), TestHelpers.CreateItem(Guid.Empty) }).ToList();
		var feed = new CachedFeedMock(storage);
		var task = feed.LoadAsync();
		Assert.True(task.IsCompleted);
		Assert.Equal(2, feed.Items.Count);
	}

	[Fact]
	public void SynchronizeItems()
	{
		var storage = new ItemStorageMock();
		var feed = new CachedFeedMock(storage);
		feed.LoadAsync();
		var taskA = feed.SynchronizeAsync();
		Assert.False(taskA.IsCompleted);
		feed.CompleteFetch(TestHelpers.CreateItem(Guid.NewGuid()));
		Assert.True(taskA.IsCompleted);
		Assert.Single(feed.Items);
		var taskB = feed.SynchronizeAsync();
		Assert.False(taskB.IsCompleted);
		feed.CompleteFetch(TestHelpers.CreateItem(Guid.NewGuid()));
		Assert.True(taskB.IsCompleted);
		Assert.Equal(2, feed.Items.Count);
	}
}
