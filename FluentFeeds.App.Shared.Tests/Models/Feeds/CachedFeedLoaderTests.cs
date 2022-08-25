using System;
using System.Linq;
using System.Threading.Tasks;
using FluentFeeds.App.Shared.Models.Feeds.Loaders;
using FluentFeeds.App.Shared.Tests.Mock;
using FluentFeeds.Documents;
using FluentFeeds.Feeds.Base.Feeds.Content;
using FluentFeeds.Feeds.Base.Items;
using FluentFeeds.Feeds.Base.Items.Content;
using Xunit;

namespace FluentFeeds.App.Shared.Tests.Models.Feeds;

public class CachedFeedLoaderTests
{
	private static ItemDescriptor CreateItemDescriptor()
	{
		return new ItemDescriptor(
			identifier: null,
			title: String.Empty,
			author: null,
			summary: null,
			publishedTimestamp: DateTimeOffset.Now,
			modifiedTimestamp: DateTimeOffset.Now,
			url: null,
			contentUrl: null,
			contentLoader: new StaticItemContentLoader(new ArticleItemContent(new RichText())));
	}
	
	[Fact]
	public async Task Initialize()
	{
		var storage = new ItemStorageMock();
		var contentLoader = new FeedContentLoaderMock(String.Empty);
		_ = storage.AddItems(new[] { CreateItemDescriptor(), CreateItemDescriptor() }, Guid.Empty).ToList();
		var feed = new CachedFeedLoader(Guid.Empty, storage, contentLoader);
		await feed.InitializeAsync();
		Assert.Equal(2, feed.Items.Count);
	}

	[Fact]
	public async Task Synchronize()
	{
		var storage = new ItemStorageMock();
		var contentLoader = new FeedContentLoaderMock(String.Empty);
		var feed = new CachedFeedLoader(Guid.Empty, storage, contentLoader);
		_ = feed.InitializeAsync();
		var taskA = feed.SynchronizeAsync();
		Assert.False(taskA.IsCompleted);
		contentLoader.CompleteLoad(new FeedContent(new FeedMetadata(), CreateItemDescriptor()));
		await taskA;
		Assert.Single(feed.Items);
		var taskB = feed.SynchronizeAsync();
		Assert.False(taskB.IsCompleted);
		contentLoader.CompleteLoad(new FeedContent(new FeedMetadata(), CreateItemDescriptor()));
		await taskB;
		Assert.Equal(2, feed.Items.Count);
	}

	[Fact]
	public async Task DeleteItem()
	{
		var storage = new ItemStorageMock();
		var contentLoader = new FeedContentLoaderMock(String.Empty);
		var items = storage.AddItems(new[] { CreateItemDescriptor(), CreateItemDescriptor() }, Guid.Empty).ToList();
		var feed = new CachedFeedLoader(Guid.Empty, storage, contentLoader);
		await feed.InitializeAsync();
		storage.DeleteItems(new[] { items[1].Identifier });
		var item = Assert.Single(feed.Items);
		Assert.Equal(items[0], item);
	}
}
