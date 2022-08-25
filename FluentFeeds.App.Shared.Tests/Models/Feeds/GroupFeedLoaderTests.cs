using System;
using System.Collections.Immutable;
using FluentFeeds.App.Shared.Models.Feeds;
using FluentFeeds.App.Shared.Models.Feeds.Loaders;
using FluentFeeds.App.Shared.Tests.Mock;
using FluentFeeds.Feeds.Base.Feeds.Content;
using Xunit;

namespace FluentFeeds.App.Shared.Tests.Models.Feeds;

public class GroupFeedLoaderTests
{
	[Fact]
	public void Initialize()
	{
		var feedA = new FeedLoaderMock();
		var feedB = new FeedLoaderMock();
		var feed = new GroupFeedLoader(ImmutableHashSet.Create<FeedLoader>(feedA, feedB));
		var task = feed.InitializeAsync();
		Assert.False(task.IsCompleted);
		feedA.CompleteInitialize(FeedLoaderTests.CreateItem(Guid.NewGuid()));
		Assert.False(task.IsCompleted);
		Assert.Empty(feed.Items);
		feedB.CompleteInitialize(FeedLoaderTests.CreateItem(Guid.NewGuid()));
		Assert.True(task.IsCompleted);
		Assert.Equal(2, feed.Items.Count);
	}
	
	[Fact]
	public void Synchronize()
	{
		var feedA = new FeedLoaderMock();
		feedA.InitializeAsync();
		feedA.CompleteInitialize();
		var feedB = new FeedLoaderMock();
		feedB.InitializeAsync();
		feedB.CompleteInitialize();
		var feed = new GroupFeedLoader(ImmutableHashSet.Create<FeedLoader>(feedA, feedB));
		var task = feed.SynchronizeAsync();
		Assert.False(task.IsCompleted);
		feedA.CompleteSynchronize(FeedLoaderTests.CreateItem(Guid.NewGuid()));
		Assert.False(task.IsCompleted);
		Assert.Empty(feed.Items);
		feedB.CompleteSynchronize(FeedLoaderTests.CreateItem(Guid.NewGuid()));
		Assert.True(task.IsCompleted);
		Assert.Equal(2, feed.Items.Count);
	}

	[Fact]
	public void UpdateItems()
	{
		var feedA = new FeedLoaderMock();
		feedA.InitializeAsync();
		feedA.CompleteInitialize(FeedLoaderTests.CreateItem(Guid.NewGuid()));
		var feedB = new FeedLoaderMock();
		feedB.InitializeAsync();
		feedB.CompleteInitialize();
		var feed = new GroupFeedLoader(ImmutableHashSet.Create<FeedLoader>(feedA, feedB));
		feed.InitializeAsync();
		feedB.UpdateItems(FeedLoaderTests.CreateItem(Guid.NewGuid()));
		Assert.Equal(2, feed.Items.Count);
	}

	[Fact]
	public void UpdateItems_BeforeLoad()
	{
		var feedA = new FeedLoaderMock();
		var feedB = new FeedLoaderMock();
		var feed = new GroupFeedLoader(ImmutableHashSet.Create<FeedLoader>(feedA, feedB));
		feedB.UpdateItems(FeedLoaderTests.CreateItem(Guid.NewGuid()));
		Assert.Empty(feed.Items);
	}

	[Fact]
	public void UpdateFeeds_Unchanged()
	{
		var feedA = new FeedLoaderMock();
		feedA.InitializeAsync();
		feedA.CompleteInitialize(FeedLoaderTests.CreateItem(Guid.NewGuid()));
		var feedB = new FeedLoaderMock();
		feedB.InitializeAsync();
		feedB.CompleteInitialize(FeedLoaderTests.CreateItem(Guid.NewGuid()));
		var feed = new GroupFeedLoader(ImmutableHashSet.Create<FeedLoader>(feedA, feedB));
		feed.InitializeAsync();
		feed.Loaders = ImmutableHashSet.Create<FeedLoader>(feedA, feedB);
		Assert.Equal(2, feed.Items.Count);
	}

	[Fact]
	public void UpdateFeeds_RemoveFeed()
	{
		var feedA = new FeedLoaderMock();
		feedA.InitializeAsync();
		feedA.CompleteInitialize(FeedLoaderTests.CreateItem(Guid.NewGuid()));
		var feedB = new FeedLoaderMock();
		feedB.InitializeAsync();
		feedB.CompleteInitialize(FeedLoaderTests.CreateItem(Guid.NewGuid()));
		var feed = new GroupFeedLoader(ImmutableHashSet.Create<FeedLoader>(feedA, feedB));
		feed.InitializeAsync();
		feed.Loaders = ImmutableHashSet.Create<FeedLoader>(feedA);
		Assert.Single(feed.Items);
		feedB.UpdateItems(FeedLoaderTests.CreateItem(Guid.NewGuid()), FeedLoaderTests.CreateItem(Guid.NewGuid()));
		Assert.Single(feed.Items);
		feedA.UpdateItems(FeedLoaderTests.CreateItem(Guid.NewGuid()), FeedLoaderTests.CreateItem(Guid.NewGuid()));
		Assert.Equal(2, feed.Items.Count);
	}

	[Fact]
	public void UpdateFeeds_AddFeed_BeforeInitialization()
	{
		var feedA = new FeedLoaderMock();
		feedA.InitializeAsync();
		feedA.CompleteInitialize(FeedLoaderTests.CreateItem(Guid.NewGuid()));
		var feedB = new FeedLoaderMock();
		feedB.InitializeAsync();
		feedB.CompleteInitialize(FeedLoaderTests.CreateItem(Guid.NewGuid()));
		var feed = new GroupFeedLoader(ImmutableHashSet.Create<FeedLoader>(feedA));
		Assert.Empty(feed.Items);
		feed.Loaders = ImmutableHashSet.Create<FeedLoader>(feedA, feedB);
		Assert.Empty(feed.Items);
		feed.InitializeAsync();
		Assert.Equal(2, feed.Items.Count);
	}

	[Fact]
	public void UpdateFeeds_AddFeed_DuringInitialization()
	{
		var feedA = new FeedLoaderMock();
		var feedB = new FeedLoaderMock();
		feedB.InitializeAsync();
		feedB.CompleteInitialize(FeedLoaderTests.CreateItem(Guid.NewGuid()));
		var feed = new GroupFeedLoader(ImmutableHashSet.Create<FeedLoader>(feedA));
		feed.InitializeAsync();
		feed.Loaders = ImmutableHashSet.Create<FeedLoader>(feedA, feedB);
		feedA.CompleteInitialize(FeedLoaderTests.CreateItem(Guid.NewGuid()));
		Assert.Equal(2, feed.Items.Count);
	}

	[Fact]
	public void UpdateFeeds_AddFeed_AfterInitialization()
	{
		var feedA = new FeedLoaderMock();
		feedA.InitializeAsync();
		feedA.CompleteInitialize(FeedLoaderTests.CreateItem(Guid.NewGuid()));
		var feedB = new FeedLoaderMock();
		feedB.InitializeAsync();
		feedB.CompleteInitialize(FeedLoaderTests.CreateItem(Guid.NewGuid()));
		var feed = new GroupFeedLoader(ImmutableHashSet.Create<FeedLoader>(feedA));
		feed.InitializeAsync();
		feed.Loaders = ImmutableHashSet.Create<FeedLoader>(feedA, feedB);
		Assert.Equal(2, feed.Items.Count);
	}

	[Fact]
	public void ErrorDuringInitialize()
	{
		var feedA = new FeedLoaderMock();
		var feedB = new FeedLoaderMock();
		var feed = new GroupFeedLoader(ImmutableHashSet.Create<FeedLoader>(feedA, feedB));
		var task = feed.InitializeAsync();
		feedA.CompleteInitialize(FeedLoaderTests.CreateItem(Guid.NewGuid()));
		feedB.CompleteInitialize(new NotSupportedException());
		Assert.True(task.IsCompleted);
		Assert.NotNull(task.Exception);
		Assert.Single(feed.Items);
	}

	[Fact]
	public void ErrorDuringInitialize_AddedFeed()
	{
		var feedA = new FeedLoaderMock();
		feedA.InitializeAsync();
		feedA.CompleteInitialize(FeedLoaderTests.CreateItem(Guid.NewGuid()));
		var feedB = new FeedLoaderMock();
		var feed = new GroupFeedLoader(ImmutableHashSet.Create<FeedLoader>(feedA));
		feed.InitializeAsync();
		feed.Loaders = ImmutableHashSet.Create<FeedLoader>(feedA, feedB);
		feedB.CompleteInitialize(new NotSupportedException());
		Assert.Single(feed.Items);
	}

	[Fact]
	public void ErrorDuringSynchronize()
	{
		var feedA = new FeedLoaderMock();
		feedA.InitializeAsync();
		feedA.CompleteInitialize();
		var feedB = new FeedLoaderMock();
		feedB.InitializeAsync();
		feedB.CompleteInitialize();
		var feed = new GroupFeedLoader(ImmutableHashSet.Create<FeedLoader>(feedA, feedB));
		var task = feed.SynchronizeAsync();
		feedA.CompleteSynchronize(FeedLoaderTests.CreateItem(Guid.NewGuid()));
		feedB.CompleteSynchronize(new NotSupportedException());
		Assert.True(task.IsCompleted);
		Assert.Single(feed.Items);
	}

	[Fact]
	public void CreateFromFeed()
	{
		var feedA = new Feed(
			identifier: Guid.NewGuid(),
			storage: null,
			loaderFactory: _ => new EmptyFeedLoader(),
			hasChildren: false,
			parent: null,
			name: null,
			symbol: null,
			metadata: new FeedMetadata(),
			isUserCustomizable: false,
			isExcludedFromGroup: false);
		var feedB = new Feed(
			identifier: Guid.NewGuid(),
			storage: null,
			loaderFactory: _ => new EmptyFeedLoader(),
			hasChildren: false,
			parent: null,
			name: null,
			symbol: null,
			metadata: new FeedMetadata(),
			isUserCustomizable: false,
			isExcludedFromGroup: false);
		var feedC = new Feed(
			identifier: Guid.NewGuid(),
			storage: null,
			loaderFactory: _ => new EmptyFeedLoader(),
			hasChildren: false,
			parent: null,
			name: null,
			symbol: null,
			metadata: new FeedMetadata(),
			isUserCustomizable: false,
			isExcludedFromGroup: true);
		var feed = new Feed(
			identifier: Guid.NewGuid(),
			storage: null,
			loaderFactory: GroupFeedLoader.FromFeed,
			hasChildren: true,
			parent: null,
			name: null,
			symbol: null,
			metadata: new FeedMetadata(),
			isUserCustomizable: false,
			isExcludedFromGroup: false);
		
		feed.Children!.Add(feedA);
		var loader = Assert.IsType<GroupFeedLoader>(feed.Loader);
		Assert.Single(loader.Loaders);
		Assert.Contains(feedA.Loader, loader.Loaders);
		
		feed.Children!.Add(feedB);
		Assert.Equal(2, loader.Loaders.Count);
		Assert.Contains(feedA.Loader, loader.Loaders);
		Assert.Contains(feedB.Loader, loader.Loaders);
		
		feed.Children!.Add(feedC);
		Assert.Equal(2, loader.Loaders.Count);
		Assert.Contains(feedA.Loader, loader.Loaders);
		Assert.Contains(feedB.Loader, loader.Loaders);
	}
}
