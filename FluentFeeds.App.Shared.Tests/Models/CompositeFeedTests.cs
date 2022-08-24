using System;
using System.Collections.Immutable;
using FluentFeeds.App.Shared.Models.Feeds;
using FluentFeeds.App.Shared.Models.Feeds.Loaders;
using FluentFeeds.Feeds.Base.Tests.Mock;
using Xunit;

namespace FluentFeeds.Feeds.Base.Tests;

public class CompositeFeedTests
{
	private ItemStorageMock ItemStorage { get; } = new();
	
	[Fact]
	public void LoadItems()
	{
		var feedA = new FeedMock();
		var feedB = new FeedMock();
		var feed = new CompositeFeed(feedA, feedB);
		var task = feed.LoadAsync();
		Assert.False(task.IsCompleted);
		feedA.CompleteLoad(TestHelpers.CreateItem(Guid.NewGuid(), ItemStorage));
		Assert.False(task.IsCompleted);
		Assert.Empty(feed.Items);
		feedB.CompleteLoad(TestHelpers.CreateItem(Guid.NewGuid(), ItemStorage));
		Assert.True(task.IsCompleted);
		Assert.Equal(2, feed.Items.Count);
	}
	
	[Fact]
	public void SynchronizeItems()
	{
		var feedA = new FeedMock();
		feedA.LoadAsync();
		feedA.CompleteLoad();
		var feedB = new FeedMock();
		feedB.LoadAsync();
		feedB.CompleteLoad();
		var feed = new CompositeFeed(feedA, feedB);
		var task = feed.SynchronizeAsync();
		Assert.False(task.IsCompleted);
		feedA.CompleteSynchronize(TestHelpers.CreateItem(Guid.NewGuid(), ItemStorage));
		Assert.False(task.IsCompleted);
		Assert.Empty(feed.Items);
		feedB.CompleteSynchronize(TestHelpers.CreateItem(Guid.NewGuid(), ItemStorage));
		Assert.True(task.IsCompleted);
		Assert.Equal(2, feed.Items.Count);
	}

	[Fact]
	public void UpdateItems()
	{
		var feedA = new FeedMock();
		feedA.LoadAsync();
		feedA.CompleteLoad(TestHelpers.CreateItem(Guid.NewGuid(), ItemStorage));
		var feedB = new FeedMock();
		feedB.LoadAsync();
		feedB.CompleteLoad();
		var feed = new CompositeFeed(feedA, feedB);
		feed.LoadAsync();
		feedB.UpdateItems(TestHelpers.CreateItem(Guid.NewGuid(), ItemStorage));
		Assert.Equal(2, feed.Items.Count);
	}

	[Fact]
	public void UpdateItems_BeforeLoad()
	{
		var feedA = new FeedMock();
		var feedB = new FeedMock();
		var feed = new CompositeFeed(feedA, feedB);
		feedB.UpdateItems(TestHelpers.CreateItem(Guid.NewGuid(), ItemStorage));
		Assert.Empty(feed.Items);
	}

	[Fact]
	public void UpdateFeeds_Unchanged()
	{
		var feedA = new FeedMock();
		feedA.LoadAsync();
		feedA.CompleteLoad(TestHelpers.CreateItem(Guid.NewGuid(), ItemStorage));
		var feedB = new FeedMock();
		feedB.LoadAsync();
		feedB.CompleteLoad(TestHelpers.CreateItem(Guid.NewGuid(), ItemStorage));
		var feed = new CompositeFeed(feedA, feedB);
		feed.LoadAsync();
		feed.Feeds = ImmutableHashSet.Create<Feed>(feedA, feedB);
		Assert.Equal(2, feed.Items.Count);
	}

	[Fact]
	public void UpdateFeeds_RemoveFeed()
	{
		var feedA = new FeedMock();
		feedA.LoadAsync();
		feedA.CompleteLoad(TestHelpers.CreateItem(Guid.NewGuid(), ItemStorage));
		var feedB = new FeedMock();
		feedB.LoadAsync();
		feedB.CompleteLoad(TestHelpers.CreateItem(Guid.NewGuid(), ItemStorage));
		var feed = new CompositeFeed(feedA, feedB);
		feed.LoadAsync();
		feed.Feeds = ImmutableHashSet.Create<Feed>(feedA);
		Assert.Single(feed.Items);
		feedB.UpdateItems(
			TestHelpers.CreateItem(Guid.NewGuid(), ItemStorage), TestHelpers.CreateItem(Guid.NewGuid(), ItemStorage));
		Assert.Single(feed.Items);
		feedA.UpdateItems(
			TestHelpers.CreateItem(Guid.NewGuid(), ItemStorage), TestHelpers.CreateItem(Guid.NewGuid(), ItemStorage));
		Assert.Equal(2, feed.Items.Count);
	}

	[Fact]
	public void UpdateFeeds_AddFeed_BeforeLoad()
	{
		var feedA = new FeedMock();
		feedA.LoadAsync();
		feedA.CompleteLoad(TestHelpers.CreateItem(Guid.NewGuid(), ItemStorage));
		var feedB = new FeedMock();
		feedB.LoadAsync();
		feedB.CompleteLoad(TestHelpers.CreateItem(Guid.NewGuid(), ItemStorage));
		var feed = new CompositeFeed(feedA);
		Assert.Empty(feed.Items);
		feed.Feeds = ImmutableHashSet.Create<Feed>(feedA, feedB);
		Assert.Empty(feed.Items);
		feed.LoadAsync();
		Assert.Equal(2, feed.Items.Count);
	}

	[Fact]
	public void UpdateFeeds_AddFeed_DuringLoad()
	{
		var feedA = new FeedMock();
		var feedB = new FeedMock();
		feedB.LoadAsync();
		feedB.CompleteLoad(TestHelpers.CreateItem(Guid.NewGuid(), ItemStorage));
		var feed = new CompositeFeed(feedA);
		feed.LoadAsync();
		feed.Feeds = ImmutableHashSet.Create<Feed>(feedA, feedB);
		feedA.CompleteLoad(TestHelpers.CreateItem(Guid.NewGuid(), ItemStorage));
		Assert.Equal(2, feed.Items.Count);
	}

	[Fact]
	public void UpdateFeeds_AddFeed_AfterLoad()
	{
		var feedA = new FeedMock();
		feedA.LoadAsync();
		feedA.CompleteLoad(TestHelpers.CreateItem(Guid.NewGuid(), ItemStorage));
		var feedB = new FeedMock();
		feedB.LoadAsync();
		feedB.CompleteLoad(TestHelpers.CreateItem(Guid.NewGuid(), ItemStorage));
		var feed = new CompositeFeed(feedA);
		feed.LoadAsync();
		feed.Feeds = ImmutableHashSet.Create<Feed>(feedA, feedB);
		Assert.Equal(2, feed.Items.Count);
	}

	[Fact]
	public void ErrorDuringLoad()
	{
		var feedA = new FeedMock();
		var feedB = new FeedMock();
		var feed = new CompositeFeed(feedA, feedB);
		var task = feed.LoadAsync();
		feedA.CompleteLoad(TestHelpers.CreateItem(Guid.NewGuid(), ItemStorage));
		feedB.CompleteLoad(new NotSupportedException());
		Assert.True(task.IsCompleted);
		Assert.NotNull(task.Exception);
		Assert.Single(feed.Items);
	}

	[Fact]
	public void ErrorDuringLoad_AddedFeed()
	{
		var feedA = new FeedMock();
		feedA.LoadAsync();
		feedA.CompleteLoad(TestHelpers.CreateItem(Guid.NewGuid(), ItemStorage));
		var feedB = new FeedMock();
		var feed = new CompositeFeed(feedA);
		feed.LoadAsync();
		feed.Feeds = ImmutableHashSet.Create<Feed>(feedA, feedB);
		feedB.CompleteLoad(new NotSupportedException());
		Assert.Single(feed.Items);
	}

	[Fact]
	public void ErrorDuringSynchronize()
	{
		var feedA = new FeedMock();
		feedA.LoadAsync();
		feedA.CompleteLoad();
		var feedB = new FeedMock();
		feedB.LoadAsync();
		feedB.CompleteLoad();
		var feed = new CompositeFeed(feedA, feedB);
		var task = feed.SynchronizeAsync();
		feedA.CompleteSynchronize(TestHelpers.CreateItem(Guid.NewGuid(), ItemStorage));
		feedB.CompleteSynchronize(new NotSupportedException());
		Assert.True(task.IsCompleted);
		Assert.Single(feed.Items);
	}
}
