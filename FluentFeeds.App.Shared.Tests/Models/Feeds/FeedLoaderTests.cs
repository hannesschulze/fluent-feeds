using System;
using FluentFeeds.App.Shared.EventArgs;
using FluentFeeds.App.Shared.Models.Items;
using FluentFeeds.App.Shared.Tests.Mock;
using FluentFeeds.Documents;
using FluentFeeds.Feeds.Base.Feeds.Content;
using FluentFeeds.Feeds.Base.Items.Content;
using Xunit;

namespace FluentFeeds.App.Shared.Tests.Models.Feeds;

public class FeedLoaderTests
{
	public static Item CreateItem(Guid identifier)
	{
		return new Item(
			identifier: identifier,
			storage: new ItemStorageMock(),
			userIdentifier: null,
			title: "title",
			author: "author",
			summary: "summary",
			publishedTimestamp: DateTimeOffset.Now,
			modifiedTimestamp: DateTimeOffset.Now,
			url: null,
			contentUrl: null,
			isRead: false,
			contentLoader: new StaticItemContentLoader(new ArticleItemContent(new RichText())));
	}
	
	[Fact]
	public void Initialize()
	{
		var feed = new FeedLoaderMock();
		Assert.Empty(feed.Items);
		var task = feed.InitializeAsync();
		Assert.False(task.IsCompleted);
		Assert.Raises<FeedItemsUpdatedEventArgs>(
			h => feed.ItemsUpdated += h,
			h => feed.ItemsUpdated -= h,
			() => feed.CompleteInitialize(CreateItem(Guid.Parse("df3ba29e-f58f-476c-aae2-f8c35d2e0cc4"))));
		Assert.True(task.IsCompleted);
		Assert.Collection(
			feed.Items,
			item => Assert.Equal(Guid.Parse("df3ba29e-f58f-476c-aae2-f8c35d2e0cc4"), item.Identifier));
	}

	[Fact]
	public void Initialize_AlreadyInProgress()
	{
		var feed = new FeedLoaderMock();
		var taskA = feed.InitializeAsync();
		var taskB = feed.InitializeAsync();
		Assert.False(taskA.IsCompleted);
		Assert.False(taskB.IsCompleted);
		feed.CompleteInitialize(CreateItem(Guid.Parse("df3ba29e-f58f-476c-aae2-f8c35d2e0cc4")));
		Assert.True(taskA.IsCompleted);
		Assert.True(taskB.IsCompleted);
		Assert.Collection(
			feed.Items,
			item => Assert.Equal(Guid.Parse("df3ba29e-f58f-476c-aae2-f8c35d2e0cc4"), item.Identifier));
	}

	[Fact]
	public void Initialize_AlreadyDone()
	{
		var feed = new FeedLoaderMock();
		feed.InitializeAsync();
		feed.CompleteInitialize(CreateItem(Guid.Parse("df3ba29e-f58f-476c-aae2-f8c35d2e0cc4")));
		var task = feed.InitializeAsync();
		Assert.True(task.IsCompleted);
		feed.CompleteInitialize(CreateItem(Guid.Parse("bd977580-70eb-4359-b370-a41c15e1fbc8")));
		Assert.Collection(
			feed.Items,
			item => Assert.Equal(Guid.Parse("df3ba29e-f58f-476c-aae2-f8c35d2e0cc4"), item.Identifier));
	}

	[Fact]
	public void Synchronize()
	{
		var feed = new FeedLoaderMock();
		feed.InitializeAsync();
		feed.CompleteInitialize();
		var task = feed.SynchronizeAsync();
		Assert.False(task.IsCompleted);
		Assert.Raises<FeedItemsUpdatedEventArgs>(
			h => feed.ItemsUpdated += h,
			h => feed.ItemsUpdated -= h,
			() => feed.CompleteSynchronize(CreateItem(Guid.Parse("df3ba29e-f58f-476c-aae2-f8c35d2e0cc4"))));
		Assert.True(task.IsCompleted);
		Assert.Collection(
			feed.Items,
			item => Assert.Equal(Guid.Parse("df3ba29e-f58f-476c-aae2-f8c35d2e0cc4"), item.Identifier));
	}

	[Fact]
	public void Synchronize_WithoutInitializing()
	{
		var feed = new FeedLoaderMock();
		var task = feed.SynchronizeAsync();
		Assert.False(task.IsCompleted);
		Assert.Raises<FeedItemsUpdatedEventArgs>(
			h => feed.ItemsUpdated += h,
			h => feed.ItemsUpdated -= h,
			() => feed.CompleteInitialize(CreateItem(Guid.Parse("df3ba29e-f58f-476c-aae2-f8c35d2e0cc4"))));
		Assert.False(task.IsCompleted);
		Assert.Collection(
			feed.Items,
			item => Assert.Equal(Guid.Parse("df3ba29e-f58f-476c-aae2-f8c35d2e0cc4"), item.Identifier));
		Assert.Raises<FeedItemsUpdatedEventArgs>(
			h => feed.ItemsUpdated += h,
			h => feed.ItemsUpdated -= h,
			() => feed.CompleteSynchronize(CreateItem(Guid.Parse("bd977580-70eb-4359-b370-a41c15e1fbc8"))));
		Assert.True(task.IsCompleted);
		Assert.Collection(
			feed.Items,
			item => Assert.Equal(Guid.Parse("bd977580-70eb-4359-b370-a41c15e1fbc8"), item.Identifier));
	}

	[Fact]
	public void Synchronize_AlreadyInProgress()
	{
		var feed = new FeedLoaderMock();
		var taskA = feed.SynchronizeAsync();
		var taskB = feed.SynchronizeAsync();
		Assert.False(taskA.IsCompleted);
		Assert.False(taskB.IsCompleted);
		feed.CompleteInitialize();
		feed.CompleteSynchronize(CreateItem(Guid.Parse("df3ba29e-f58f-476c-aae2-f8c35d2e0cc4")));
		Assert.True(taskA.IsCompleted);
		Assert.True(taskB.IsCompleted);
		Assert.Collection(
			feed.Items,
			item => Assert.Equal(Guid.Parse("df3ba29e-f58f-476c-aae2-f8c35d2e0cc4"), item.Identifier));
	}

	[Fact]
	public void Synchronize_AfterFirstCompletion()
	{
		var feed = new FeedLoaderMock();
		feed.SynchronizeAsync();
		feed.CompleteInitialize();
		feed.CompleteSynchronize(CreateItem(Guid.Parse("df3ba29e-f58f-476c-aae2-f8c35d2e0cc4")));
		var task = feed.SynchronizeAsync();
		Assert.False(task.IsCompleted);
		Assert.Raises<FeedItemsUpdatedEventArgs>(
			h => feed.ItemsUpdated += h,
			h => feed.ItemsUpdated -= h,
			() => feed.CompleteSynchronize(CreateItem(Guid.Parse("bd977580-70eb-4359-b370-a41c15e1fbc8"))));
		Assert.True(task.IsCompleted);
		Assert.Collection(
			feed.Items,
			item => Assert.Equal(Guid.Parse("bd977580-70eb-4359-b370-a41c15e1fbc8"), item.Identifier));
	}

	[Fact]
	public void UpdateItemsManually()
	{
		var feed = new FeedLoaderMock();
		feed.InitializeAsync();
		feed.CompleteInitialize();
		Assert.Raises<FeedItemsUpdatedEventArgs>(
			h => feed.ItemsUpdated += h,
			h => feed.ItemsUpdated -= h,
			() => feed.UpdateItems(CreateItem(Guid.Parse("bd977580-70eb-4359-b370-a41c15e1fbc8"))));
		Assert.Collection(
			feed.Items,
			item => Assert.Equal(Guid.Parse("bd977580-70eb-4359-b370-a41c15e1fbc8"), item.Identifier));
	}
}
