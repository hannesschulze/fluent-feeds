using System;
using System.Threading.Tasks;
using FluentFeeds.App.Shared.EventArgs;
using FluentFeeds.App.Shared.Models.Feeds;
using FluentFeeds.App.Shared.Models.Feeds.Loaders;
using FluentFeeds.App.Shared.Models.Storage;
using FluentFeeds.App.Shared.Tests.Mock;
using FluentFeeds.Common;
using FluentFeeds.Feeds.Base.Feeds;
using FluentFeeds.Feeds.Base.Feeds.Content;
using Xunit;
using Xunit.Abstractions;

namespace FluentFeeds.App.Shared.Tests.Models.Storage;

public class FeedStorageTests
{
	public FeedStorageTests(ITestOutputHelper testOutputHelper)
	{
		DatabaseService = new DatabaseServiceMock(testOutputHelper);
	}
	
	private DatabaseServiceMock DatabaseService { get; }
	
	private Task<IFeedView> CreateRootFeedAsync(Guid? identifier = null)
	{
		var provider = new FeedProviderMock(identifier ?? Guid.Parse("ab810ada-234c-46dd-a62d-0d76c61f29f1"));
		var storage = new FeedStorage(DatabaseService, new ItemContentCache(5), provider);
		return DatabaseService.ExecuteAsync(
			async database =>
			{
				var result = await storage.InitializeAsync(database);
				await database.SaveChangesAsync();
				return result;
			});
	}

	[Fact]
	public async Task ReuseItemStorages()
	{
		var node = await CreateRootFeedAsync();
		var storageA = node.Storage!.GetItemStorage(Guid.Parse("45bdb8dc-c27c-48ce-978c-158d1a78d5bc"));
		var storageB = node.Storage!.GetItemStorage(Guid.Parse("45bdb8dc-c27c-48ce-978c-158d1a78d5bc"));
		var storageC = node.Storage!.GetItemStorage(Guid.Parse("5956070c-39d3-4e69-bd30-6199b6399d6d"));
		Assert.Equal(storageA, storageB);
		Assert.NotEqual(storageA, storageC);
	}

	[Theory]
	[InlineData(null)]
	[InlineData("8724a7bd-a064-419b-b49f-2aeee22279a4")]
	public async Task AddChildFeed(string? itemStorageIdentifier)
	{
		var rootNodeA = await CreateRootFeedAsync();
		var contentLoaderA = new FeedContentLoaderMock("test");
		contentLoaderA.CompleteLoad(new FeedContent(new FeedMetadata { Name = "metadata name" }));
		var descriptor = 
			new CachedFeedDescriptor(contentLoaderA) 
			{
				Name = "feed",
				Symbol = Symbol.Web,
				ItemCacheIdentifier = itemStorageIdentifier != null ? Guid.Parse(itemStorageIdentifier) : null
			};
		var feedA = await rootNodeA.Storage!.AddFeedAsync(descriptor, rootNodeA.Identifier, syncFirst: true);
		Assert.Collection(
			rootNodeA.Children!,
			feed => Assert.Equal(feedA, feed));
		Assert.Equal(feedA, feedA.Storage!.GetFeed(feedA.Identifier));
		Assert.Equal(rootNodeA, feedA.Parent);
		Assert.Equal("feed", feedA.Name);
		Assert.Equal("metadata name", feedA.Metadata.Name);
		Assert.Equal(Symbol.Web, feedA.Symbol);
		Assert.True(feedA.IsUserCustomizable);
		Assert.False(feedA.IsExcludedFromGroup);
		Assert.Null(feedA.Children);
		Assert.NotNull(feedA.Loader.LastSynchronized);
		var loaderA = Assert.IsType<CachedFeedLoader>(feedA.Loader);
		Assert.Equal(feedA.Identifier, loaderA.FeedIdentifier);
		Assert.Equal(
			itemStorageIdentifier != null ? Guid.Parse(itemStorageIdentifier) : feedA.Identifier,
			loaderA.Storage.Identifier);

		var rootNodeB = await CreateRootFeedAsync();
		var feedB = Assert.Single(rootNodeB.Children!);
		Assert.Equal(feedA.Identifier, feedB.Identifier);
		Assert.Equal(feedB, feedB.Storage!.GetFeed(feedB.Identifier));
		Assert.Equal(rootNodeB, feedB.Parent);
		Assert.Equal("feed", feedB.Name);
		Assert.Equal("metadata name", feedB.Metadata.Name);
		Assert.Equal(Symbol.Web, feedB.Symbol);
		Assert.True(feedB.IsUserCustomizable);
		Assert.False(feedB.IsExcludedFromGroup);
		Assert.Null(feedB.Children);
		Assert.Null(feedB.Loader.LastSynchronized);
		var loaderB = Assert.IsType<CachedFeedLoader>(feedB.Loader);
		Assert.Equal(feedB.Identifier, loaderB.FeedIdentifier);
		Assert.Equal(
			itemStorageIdentifier != null ? Guid.Parse(itemStorageIdentifier) : feedB.Identifier,
			loaderB.Storage.Identifier);
	}

	[Fact]
	public async Task AddChildFeed_Sorted()
	{
		var rootNode = await CreateRootFeedAsync();
		await rootNode.Storage!.AddFeedAsync(new GroupFeedDescriptor("baz", Symbol.Web), rootNode.Identifier);
		await rootNode.Storage!.AddFeedAsync(new GroupFeedDescriptor("bar", Symbol.Web), rootNode.Identifier);
		await rootNode.Storage!.AddFeedAsync(new GroupFeedDescriptor("foo", Symbol.Web), rootNode.Identifier);
		Assert.Collection(
			rootNode.Children!,
			node => Assert.Equal("bar", node.Name),
			node => Assert.Equal("baz", node.Name),
			node => Assert.Equal("foo", node.Name));
	}

	[Fact]
	public async Task RenameFeed()
	{
		var rootNodeA = await CreateRootFeedAsync();
		var feedA = await rootNodeA.Storage!.AddFeedAsync(
			new GroupFeedDescriptor("feed", Symbol.Web), rootNodeA.Identifier);
		await feedA.Storage!.RenameFeedAsync(feedA.Identifier, "updated");
		Assert.Equal("updated", feedA.Name);

		var rootNodeB = await CreateRootFeedAsync();
		var feedB = Assert.Single(rootNodeB.Children!);
		Assert.Equal("updated", feedB.Name);
	}

	[Fact]
	public async Task UpdateFeedMetadata()
	{
		var rootNodeA = await CreateRootFeedAsync();
		var contentLoader = new FeedContentLoaderMock("test");
		var feedA = await rootNodeA.Storage!.AddFeedAsync(
			new CachedFeedDescriptor(contentLoader), rootNodeA.Identifier);
		var syncTask = feedA.Loader.SynchronizeAsync();
		contentLoader.CompleteLoad(new FeedContent(
			new FeedMetadata { Name = "name", Description = "description", Author = "author", Symbol = Symbol.Web }));
		await syncTask;
		Assert.Equal("name", feedA.Metadata.Name);
		Assert.Equal("description", feedA.Metadata.Description);
		Assert.Equal("author", feedA.Metadata.Author);
		Assert.Equal(Symbol.Web, feedA.Metadata.Symbol);

		var rootNodeB = await CreateRootFeedAsync();
		var feedB = Assert.Single(rootNodeB.Children!);
		Assert.Equal("name", feedB.Metadata.Name);
		Assert.Equal("description", feedB.Metadata.Description);
		Assert.Equal("author", feedB.Metadata.Author);
		Assert.Equal(Symbol.Web, feedB.Metadata.Symbol);
	}

	[Fact]
	public async Task MoveFeed_SameParent()
	{
		var rootNodeA = await CreateRootFeedAsync();
		var parentA = await rootNodeA.Storage!.AddFeedAsync(
			new GroupFeedDescriptor("parent", Symbol.Web), rootNodeA.Identifier);
		var childA = await rootNodeA.Storage!.AddFeedAsync(
			new GroupFeedDescriptor("child", Symbol.Web), parentA.Identifier);
		await childA.Storage!.MoveFeedAsync(childA.Identifier, parentA.Identifier);
		Assert.Collection(
			parentA.Children!,
			node => Assert.Equal(childA, node));
		Assert.Equal(parentA, childA.Parent);

		var rootNodeB = await CreateRootFeedAsync();
		var parentB = Assert.Single(rootNodeB.Children!);
		var childB = Assert.Single(parentB.Children!);
		Assert.Equal(parentB, childB.Parent);
	}

	[Fact]
	public async Task MoveFeed_DifferentParent()
	{
		var rootNodeA = await CreateRootFeedAsync();
		var parentA = await rootNodeA.Storage!.AddFeedAsync(
			new GroupFeedDescriptor("parent", Symbol.Web), rootNodeA.Identifier);
		var childA = await rootNodeA.Storage!.AddFeedAsync(
			new GroupFeedDescriptor("child", Symbol.Web), rootNodeA.Identifier);
		await childA.Storage!.MoveFeedAsync(childA.Identifier, parentA.Identifier);
		Assert.Collection(
			rootNodeA.Children!,
			feed => Assert.Equal(parentA, feed));
		Assert.Collection(
			parentA.Children!,
			feed => Assert.Equal(childA, feed));
		Assert.Equal(parentA, childA.Parent);

		var rootNodeB = await CreateRootFeedAsync();
		var parentB = Assert.Single(rootNodeB.Children!);
		var childB = Assert.Single(parentB.Children!);
		Assert.Equal(parentB, childB.Parent);
	}

	[Fact]
	public async Task DeleteFeed()
	{
		var rootNodeA = await CreateRootFeedAsync();
		var parentA = await rootNodeA.Storage!.AddFeedAsync(
			new GroupFeedDescriptor("parent", Symbol.Web), rootNodeA.Identifier);
		var childA = await rootNodeA.Storage!.AddFeedAsync(
			new GroupFeedDescriptor("child", Symbol.Web), parentA.Identifier);
		var deletedArgs = await Assert.RaisesAsync<FeedsDeletedEventArgs>(
			h => rootNodeA.Storage.FeedsDeleted += h, h => rootNodeA.Storage.FeedsDeleted -= h,
			() => rootNodeA.Storage.DeleteFeedAsync(parentA.Identifier));
		Assert.Collection(
			deletedArgs.Arguments.Feeds,
			node => Assert.Equal(parentA, node),
			node => Assert.Equal(childA, node));
		Assert.Empty(rootNodeA.Children!);
		Assert.Null(rootNodeA.Storage!.GetFeed(parentA.Identifier));
		Assert.Null(rootNodeA.Storage!.GetFeed(childA.Identifier));
		
		var rootNodeB = await CreateRootFeedAsync();
		Assert.Empty(rootNodeB.Children!);
		Assert.Null(rootNodeB.Storage!.GetFeed(parentA.Identifier));
		Assert.Null(rootNodeB.Storage!.GetFeed(childA.Identifier));
	}
}
