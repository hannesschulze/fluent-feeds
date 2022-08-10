using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using FluentFeeds.Documents;
using FluentFeeds.Feeds.Base.Items;
using FluentFeeds.Feeds.Base.Items.Content;
using Xunit;

namespace FluentFeeds.Feeds.Base.Tests;

public class FeedTests
{
	private sealed class TestFeed : Feed
	{
		public void CompleteLoad(IEnumerable<IReadOnlyStoredItem> items) => _loadCompletionSource?.TrySetResult(items);
		public void CompleteLoad(params IReadOnlyStoredItem[] items) => CompleteLoad(items.AsEnumerable());

		public void CompleteSynchronize(IEnumerable<IReadOnlyStoredItem> items) =>
			_synchronizeCompletionSource?.TrySetResult(items);
		public void CompleteSynchronize(params IReadOnlyStoredItem[] items) =>
			CompleteSynchronize(items.AsEnumerable());

		public void UpdateItems(params IReadOnlyStoredItem[] items) => Items = items.ToImmutableHashSet();

		public void UpdateMetadata(FeedMetadata? metadata) => Metadata = metadata;

		protected override Task<IEnumerable<IReadOnlyStoredItem>> DoLoadAsync()
		{
			var completionSource = _loadCompletionSource = new TaskCompletionSource<IEnumerable<IReadOnlyStoredItem>>();
			return completionSource.Task;
		}

		protected override Task<IEnumerable<IReadOnlyStoredItem>> DoSynchronizeAsync()
		{
			var completionSource = _synchronizeCompletionSource =
				new TaskCompletionSource<IEnumerable<IReadOnlyStoredItem>>();
			return completionSource.Task;
		}

		private TaskCompletionSource<IEnumerable<IReadOnlyStoredItem>>? _loadCompletionSource;
		private TaskCompletionSource<IEnumerable<IReadOnlyStoredItem>>? _synchronizeCompletionSource;
	}

	private static IReadOnlyStoredItem CreateItem(Guid identifier) =>
		new StoredItem(
			identifier, new Uri("https://www.example.com"), null, DateTimeOffset.Now, DateTimeOffset.Now, "title",
			"author", "summary", new ArticleItemContent(new RichText()), false);

	[Fact]
	public void LoadItems()
	{
		var feed = new TestFeed();
		Assert.Empty(feed.Items);
		var task = feed.LoadAsync();
		Assert.False(task.IsCompleted);
		Assert.Raises<EventArgs>(
			h => feed.ItemsUpdated += h,
			h => feed.ItemsUpdated -= h,
			() => feed.CompleteLoad(CreateItem(Guid.Parse("df3ba29e-f58f-476c-aae2-f8c35d2e0cc4"))));
		Assert.True(task.IsCompleted);
		Assert.Collection(
			feed.Items,
			item => Assert.Equal(Guid.Parse("df3ba29e-f58f-476c-aae2-f8c35d2e0cc4"), item.Identifier));
	}

	[Fact]
	public void LoadItems_AlreadyInProgress()
	{
		var feed = new TestFeed();
		var taskA = feed.LoadAsync();
		var taskB = feed.LoadAsync();
		Assert.False(taskA.IsCompleted);
		Assert.False(taskB.IsCompleted);
		feed.CompleteLoad(CreateItem(Guid.Parse("df3ba29e-f58f-476c-aae2-f8c35d2e0cc4")));
		Assert.True(taskA.IsCompleted);
		Assert.True(taskB.IsCompleted);
		Assert.Collection(
			feed.Items,
			item => Assert.Equal(Guid.Parse("df3ba29e-f58f-476c-aae2-f8c35d2e0cc4"), item.Identifier));
	}

	[Fact]
	public void LoadItems_AlreadyDone()
	{
		var feed = new TestFeed();
		feed.LoadAsync();
		feed.CompleteLoad(CreateItem(Guid.Parse("df3ba29e-f58f-476c-aae2-f8c35d2e0cc4")));
		var task = feed.LoadAsync();
		Assert.True(task.IsCompleted);
		feed.CompleteLoad(CreateItem(Guid.Parse("bd977580-70eb-4359-b370-a41c15e1fbc8")));
		Assert.Collection(
			feed.Items,
			item => Assert.Equal(Guid.Parse("df3ba29e-f58f-476c-aae2-f8c35d2e0cc4"), item.Identifier));
	}

	[Fact]
	public void SynchronizeItems()
	{
		var feed = new TestFeed();
		feed.LoadAsync();
		feed.CompleteLoad();
		var task = feed.SynchronizeAsync();
		Assert.False(task.IsCompleted);
		Assert.Raises<EventArgs>(
			h => feed.ItemsUpdated += h,
			h => feed.ItemsUpdated -= h,
			() => feed.CompleteSynchronize(CreateItem(Guid.Parse("df3ba29e-f58f-476c-aae2-f8c35d2e0cc4"))));
		Assert.True(task.IsCompleted);
		Assert.Collection(
			feed.Items,
			item => Assert.Equal(Guid.Parse("df3ba29e-f58f-476c-aae2-f8c35d2e0cc4"), item.Identifier));
	}

	[Fact]
	public void SynchronizeItems_WithoutLoading()
	{
		var feed = new TestFeed();
		var task = feed.SynchronizeAsync();
		Assert.False(task.IsCompleted);
		Assert.Raises<EventArgs>(
			h => feed.ItemsUpdated += h,
			h => feed.ItemsUpdated -= h,
			() => feed.CompleteLoad(CreateItem(Guid.Parse("df3ba29e-f58f-476c-aae2-f8c35d2e0cc4"))));
		Assert.False(task.IsCompleted);
		Assert.Collection(
			feed.Items,
			item => Assert.Equal(Guid.Parse("df3ba29e-f58f-476c-aae2-f8c35d2e0cc4"), item.Identifier));
		Assert.Raises<EventArgs>(
			h => feed.ItemsUpdated += h,
			h => feed.ItemsUpdated -= h,
			() => feed.CompleteSynchronize(CreateItem(Guid.Parse("bd977580-70eb-4359-b370-a41c15e1fbc8"))));
		Assert.True(task.IsCompleted);
		Assert.Collection(
			feed.Items,
			item => Assert.Equal(Guid.Parse("bd977580-70eb-4359-b370-a41c15e1fbc8"), item.Identifier));
	}

	[Fact]
	public void SynchronizeItems_AlreadyInProgress()
	{
		var feed = new TestFeed();
		var taskA = feed.SynchronizeAsync();
		var taskB = feed.SynchronizeAsync();
		Assert.False(taskA.IsCompleted);
		Assert.False(taskB.IsCompleted);
		feed.CompleteLoad();
		feed.CompleteSynchronize(CreateItem(Guid.Parse("df3ba29e-f58f-476c-aae2-f8c35d2e0cc4")));
		Assert.True(taskA.IsCompleted);
		Assert.True(taskB.IsCompleted);
		Assert.Collection(
			feed.Items,
			item => Assert.Equal(Guid.Parse("df3ba29e-f58f-476c-aae2-f8c35d2e0cc4"), item.Identifier));
	}

	[Fact]
	public void SynchronizeItems_AfterFirstCompletion()
	{
		var feed = new TestFeed();
		feed.SynchronizeAsync();
		feed.CompleteLoad();
		feed.CompleteSynchronize(CreateItem(Guid.Parse("df3ba29e-f58f-476c-aae2-f8c35d2e0cc4")));
		var task = feed.SynchronizeAsync();
		Assert.False(task.IsCompleted);
		Assert.Raises<EventArgs>(
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
		var feed = new TestFeed();
		feed.LoadAsync();
		feed.CompleteLoad();
		Assert.Raises<EventArgs>(
			h => feed.ItemsUpdated += h,
			h => feed.ItemsUpdated -= h,
			() => feed.UpdateItems(CreateItem(Guid.Parse("bd977580-70eb-4359-b370-a41c15e1fbc8"))));
		Assert.Collection(
			feed.Items,
			item => Assert.Equal(Guid.Parse("bd977580-70eb-4359-b370-a41c15e1fbc8"), item.Identifier));
	}

	[Fact]
	public void UpdateMetadata()
	{
		var feed = new TestFeed();
		Assert.Null(feed.Metadata);
		var metadata = new FeedMetadata("feed", "author", "description", null);
		Assert.Raises<EventArgs>(
			h => feed.MetadataUpdated += h,
			h => feed.MetadataUpdated -= h,
			() => feed.UpdateMetadata(metadata));
		Assert.Equal(metadata, feed.Metadata);
	}
}
