using System;
using FluentFeeds.Feeds.Base.EventArgs;
using FluentFeeds.Feeds.Base.Feeds.Content;
using FluentFeeds.Feeds.Base.Tests.Mock;
using Xunit;

namespace FluentFeeds.Feeds.Base.Tests;

public class FeedTests
{
	private ItemStorageMock ItemStorage { get; } = new();
	
	[Fact]
	public void LoadItems()
	{
		var feed = new FeedMock();
		Assert.Empty(feed.Items);
		var task = feed.LoadAsync();
		Assert.False(task.IsCompleted);
		Assert.Raises<FeedItemsUpdatedEventArgs>(
			h => feed.ItemsUpdated += h,
			h => feed.ItemsUpdated -= h,
			() => feed.CompleteLoad(
				TestHelpers.CreateItem(Guid.Parse("df3ba29e-f58f-476c-aae2-f8c35d2e0cc4"), ItemStorage)));
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
		feed.CompleteLoad(TestHelpers.CreateItem(Guid.Parse("df3ba29e-f58f-476c-aae2-f8c35d2e0cc4"), ItemStorage));
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
		feed.CompleteLoad(TestHelpers.CreateItem(Guid.Parse("df3ba29e-f58f-476c-aae2-f8c35d2e0cc4"), ItemStorage));
		var task = feed.LoadAsync();
		Assert.True(task.IsCompleted);
		feed.CompleteLoad(TestHelpers.CreateItem(Guid.Parse("bd977580-70eb-4359-b370-a41c15e1fbc8"), ItemStorage));
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
		Assert.Raises<FeedItemsUpdatedEventArgs>(
			h => feed.ItemsUpdated += h,
			h => feed.ItemsUpdated -= h,
			() => feed.CompleteSynchronize(
				TestHelpers.CreateItem(Guid.Parse("df3ba29e-f58f-476c-aae2-f8c35d2e0cc4"), ItemStorage)));
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
		Assert.Raises<FeedItemsUpdatedEventArgs>(
			h => feed.ItemsUpdated += h,
			h => feed.ItemsUpdated -= h,
			() => feed.CompleteLoad(
				TestHelpers.CreateItem(Guid.Parse("df3ba29e-f58f-476c-aae2-f8c35d2e0cc4"), ItemStorage)));
		Assert.False(task.IsCompleted);
		Assert.Collection(
			feed.Items,
			item => Assert.Equal(Guid.Parse("df3ba29e-f58f-476c-aae2-f8c35d2e0cc4"), item.Identifier));
		Assert.Raises<FeedItemsUpdatedEventArgs>(
			h => feed.ItemsUpdated += h,
			h => feed.ItemsUpdated -= h,
			() => feed.CompleteSynchronize(
				TestHelpers.CreateItem(Guid.Parse("bd977580-70eb-4359-b370-a41c15e1fbc8"), ItemStorage)));
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
		feed.CompleteSynchronize(
			TestHelpers.CreateItem(Guid.Parse("df3ba29e-f58f-476c-aae2-f8c35d2e0cc4"), ItemStorage));
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
		feed.CompleteSynchronize(
			TestHelpers.CreateItem(Guid.Parse("df3ba29e-f58f-476c-aae2-f8c35d2e0cc4"), ItemStorage));
		var task = feed.SynchronizeAsync();
		Assert.False(task.IsCompleted);
		Assert.Raises<FeedItemsUpdatedEventArgs>(
			h => feed.ItemsUpdated += h,
			h => feed.ItemsUpdated -= h,
			() => feed.CompleteSynchronize(
				TestHelpers.CreateItem(Guid.Parse("bd977580-70eb-4359-b370-a41c15e1fbc8"), ItemStorage)));
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
		Assert.Raises<FeedItemsUpdatedEventArgs>(
			h => feed.ItemsUpdated += h,
			h => feed.ItemsUpdated -= h,
			() => feed.UpdateItems(
				TestHelpers.CreateItem(Guid.Parse("bd977580-70eb-4359-b370-a41c15e1fbc8"), ItemStorage)));
		Assert.Collection(
			feed.Items,
			item => Assert.Equal(Guid.Parse("bd977580-70eb-4359-b370-a41c15e1fbc8"), item.Identifier));
	}

	[Fact]
	public void UpdateMetadata()
	{
		var feed = new FeedMock();
		Assert.Equal(new FeedMetadata(), feed.Metadata);
		var metadata = new FeedMetadata { Name = "feed", Author = "author", Description = "description" };
		Assert.Raises<FeedMetadataUpdatedEventArgs>(
			h => feed.MetadataUpdated += h,
			h => feed.MetadataUpdated -= h,
			() => feed.UpdateMetadata(metadata));
		Assert.Equal(metadata, feed.Metadata);
	}
}
