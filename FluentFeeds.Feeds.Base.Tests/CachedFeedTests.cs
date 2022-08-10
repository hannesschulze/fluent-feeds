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
		var collectionA = Guid.Parse("df3ba29e-f58f-476c-aae2-f8c35d2e0cc4");
		var collectionB = Guid.Parse("bd977580-70eb-4359-b370-a41c15e1fbc8");
		_ = storage.AddItems(
			new[] { TestHelpers.CreateItem(Guid.Empty), TestHelpers.CreateItem(Guid.Empty) }, collectionA).ToList();
		_ = storage.AddItems(new[] { TestHelpers.CreateItem(Guid.Empty) }, collectionB).ToList();
		var feed = new CachedFeedMock(storage, collectionA);
		var task = feed.LoadAsync();
		Assert.True(task.IsCompleted);
		Assert.Equal(2, feed.Items.Count);
	}

	[Fact]
	public void SynchronizeItems()
	{
		var storage = new ItemStorageMock();
		var feed = new CachedFeedMock(storage, Guid.NewGuid());
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
