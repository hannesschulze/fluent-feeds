using System;
using FluentFeeds.Feeds.Base.Tests.Mock;
using Xunit;

namespace FluentFeeds.Feeds.Base.Tests;

public class FeedTests
{
	[Fact]
	public void LoadItems()
	{
		var feed = new FeedMock();
		Assert.Empty(feed.Items);
		var task = feed.LoadAsync();
		Assert.False(task.IsCompleted);
		Assert.Raises<EventArgs>(
			h => feed.ItemsUpdated += h,
			h => feed.ItemsUpdated -= h,
			() => feed.CompleteLoad(TestHelpers.CreateItem(Guid.Parse("df3ba29e-f58f-476c-aae2-f8c35d2e0cc4"))));
		Assert.True(task.IsCompleted);
		Assert.Collection(
			feed.Items,
			item => Assert.Equal(Guid.Parse("df3ba29e-f58f-476c-aae2-f8c35d2e0cc4"), item.Identifier));
	}

	[Fact]
	public void LoadItems_AlreadyInProgress()
	{
		var feed = new FeedMock();
		var taskA = feed.LoadAsync();
		var taskB = feed.LoadAsync();
		Assert.False(taskA.IsCompleted);
		Assert.False(taskB.IsCompleted);
		feed.CompleteLoad(TestHelpers.CreateItem(Guid.Parse("df3ba29e-f58f-476c-aae2-f8c35d2e0cc4")));
		Assert.True(taskA.IsCompleted);
		Assert.True(taskB.IsCompleted);
		Assert.Collection(
			feed.Items,
			item => Assert.Equal(Guid.Parse("df3ba29e-f58f-476c-aae2-f8c35d2e0cc4"), item.Identifier));
	}

	[Fact]
	public void LoadItems_AlreadyDone()
	{
		var feed = new FeedMock();
		feed.LoadAsync();
		feed.CompleteLoad(TestHelpers.CreateItem(Guid.Parse("df3ba29e-f58f-476c-aae2-f8c35d2e0cc4")));
		var task = feed.LoadAsync();
		Assert.True(task.IsCompleted);
		feed.CompleteLoad(TestHelpers.CreateItem(Guid.Parse("bd977580-70eb-4359-b370-a41c15e1fbc8")));
		Assert.Collection(
			feed.Items,
			item => Assert.Equal(Guid.Parse("df3ba29e-f58f-476c-aae2-f8c35d2e0cc4"), item.Identifier));
	}

	[Fact]
	public void SynchronizeItems()
	{
		var feed = new FeedMock();
		feed.LoadAsync();
		feed.CompleteLoad();
		var task = feed.SynchronizeAsync();
		Assert.False(task.IsCompleted);
		Assert.Raises<EventArgs>(
			h => feed.ItemsUpdated += h,
			h => feed.ItemsUpdated -= h,
			() => feed.CompleteSynchronize(TestHelpers.CreateItem(Guid.Parse("df3ba29e-f58f-476c-aae2-f8c35d2e0cc4"))));
		Assert.True(task.IsCompleted);
		Assert.Collection(
			feed.Items,
			item => Assert.Equal(Guid.Parse("df3ba29e-f58f-476c-aae2-f8c35d2e0cc4"), item.Identifier));
	}

	[Fact]
	public void SynchronizeItems_WithoutLoading()
	{
		var feed = new FeedMock();
		var task = feed.SynchronizeAsync();
		Assert.False(task.IsCompleted);
		Assert.Raises<EventArgs>(
			h => feed.ItemsUpdated += h,
			h => feed.ItemsUpdated -= h,
			() => feed.CompleteLoad(TestHelpers.CreateItem(Guid.Parse("df3ba29e-f58f-476c-aae2-f8c35d2e0cc4"))));
		Assert.False(task.IsCompleted);
		Assert.Collection(
			feed.Items,
			item => Assert.Equal(Guid.Parse("df3ba29e-f58f-476c-aae2-f8c35d2e0cc4"), item.Identifier));
		Assert.Raises<EventArgs>(
			h => feed.ItemsUpdated += h,
			h => feed.ItemsUpdated -= h,
			() => feed.CompleteSynchronize(TestHelpers.CreateItem(Guid.Parse("bd977580-70eb-4359-b370-a41c15e1fbc8"))));
		Assert.True(task.IsCompleted);
		Assert.Collection(
			feed.Items,
			item => Assert.Equal(Guid.Parse("bd977580-70eb-4359-b370-a41c15e1fbc8"), item.Identifier));
	}

	[Fact]
	public void SynchronizeItems_AlreadyInProgress()
	{
		var feed = new FeedMock();
		var taskA = feed.SynchronizeAsync();
		var taskB = feed.SynchronizeAsync();
		Assert.False(taskA.IsCompleted);
		Assert.False(taskB.IsCompleted);
		feed.CompleteLoad();
		feed.CompleteSynchronize(TestHelpers.CreateItem(Guid.Parse("df3ba29e-f58f-476c-aae2-f8c35d2e0cc4")));
		Assert.True(taskA.IsCompleted);
		Assert.True(taskB.IsCompleted);
		Assert.Collection(
			feed.Items,
			item => Assert.Equal(Guid.Parse("df3ba29e-f58f-476c-aae2-f8c35d2e0cc4"), item.Identifier));
	}

	[Fact]
	public void SynchronizeItems_AfterFirstCompletion()
	{
		var feed = new FeedMock();
		feed.SynchronizeAsync();
		feed.CompleteLoad();
		feed.CompleteSynchronize(TestHelpers.CreateItem(Guid.Parse("df3ba29e-f58f-476c-aae2-f8c35d2e0cc4")));
		var task = feed.SynchronizeAsync();
		Assert.False(task.IsCompleted);
		Assert.Raises<EventArgs>(
			h => feed.ItemsUpdated += h,
			h => feed.ItemsUpdated -= h,
			() => feed.CompleteSynchronize(TestHelpers.CreateItem(Guid.Parse("bd977580-70eb-4359-b370-a41c15e1fbc8"))));
		Assert.True(task.IsCompleted);
		Assert.Collection(
			feed.Items,
			item => Assert.Equal(Guid.Parse("bd977580-70eb-4359-b370-a41c15e1fbc8"), item.Identifier));
	}

	[Fact]
	public void UpdateItemsManually()
	{
		var feed = new FeedMock();
		feed.LoadAsync();
		feed.CompleteLoad();
		Assert.Raises<EventArgs>(
			h => feed.ItemsUpdated += h,
			h => feed.ItemsUpdated -= h,
			() => feed.UpdateItems(TestHelpers.CreateItem(Guid.Parse("bd977580-70eb-4359-b370-a41c15e1fbc8"))));
		Assert.Collection(
			feed.Items,
			item => Assert.Equal(Guid.Parse("bd977580-70eb-4359-b370-a41c15e1fbc8"), item.Identifier));
	}

	[Fact]
	public void UpdateMetadata()
	{
		var feed = new FeedMock();
		Assert.Null(feed.Metadata);
		var metadata = new FeedMetadata("feed", "author", "description", null);
		Assert.Raises<EventArgs>(
			h => feed.MetadataUpdated += h,
			h => feed.MetadataUpdated -= h,
			() => feed.UpdateMetadata(metadata));
		Assert.Equal(metadata, feed.Metadata);
	}
}
