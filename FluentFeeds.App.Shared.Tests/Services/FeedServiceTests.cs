using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using FluentFeeds.App.Shared.EventArgs;
using FluentFeeds.App.Shared.Models.Storage;
using FluentFeeds.App.Shared.Services.Default;
using FluentFeeds.App.Shared.Tests.Mock;
using FluentFeeds.Common;
using FluentFeeds.Documents;
using FluentFeeds.Documents.Blocks;
using FluentFeeds.Documents.Inlines;
using FluentFeeds.Feeds.Base;
using FluentFeeds.Feeds.Base.Items;
using FluentFeeds.Feeds.Base.Items.Content;
using FluentFeeds.Feeds.Base.Nodes;
using Xunit;
using Xunit.Abstractions;

namespace FluentFeeds.App.Shared.Tests.Services;

public class FeedServiceTests
{
	public FeedServiceTests(ITestOutputHelper testOutputHelper)
	{
		DatabaseService = new DatabaseServiceMock(testOutputHelper);
		PluginService = new PluginServiceMock();
	}
	
	private DatabaseServiceMock DatabaseService { get; }
	private PluginServiceMock PluginService { get; }

	[Fact]
	public async Task Initialization_StoreFeedNodesForProviders()
	{
		var providerIdentifier = Guid.Parse("6ebe7ccf-e1ae-4930-9c6e-97cf61fec7e3");
		var feedIdentifier = Guid.Parse("c71e97f0-5b25-43db-908c-8e8d0b4bbe66");
		var provider =
			new FeedProviderMock(providerIdentifier)
			{
				InitialTree =
					FeedNode.Group("root", Symbol.Feed, isUserCustomizable: false, 
						FeedNode.Group("child", Symbol.Directory, isUserCustomizable: true,
							FeedNode.Custom(new FeedMock(feedIdentifier), "feed", Symbol.Web, true)))
			};
		PluginService.AvailableFeedProviders = ImmutableArray.Create<FeedProvider>(provider);
		var serviceA = new FeedService(DatabaseService, PluginService);
		await serviceA.InitializeAsync();

		var rootNodeA = Assert.Single(serviceA.ProviderNodes);
		Assert.Equal(provider, rootNodeA.Storage.Provider);
		Assert.Equal(rootNodeA, rootNodeA.Storage.GetNode(rootNodeA.Identifier));
		Assert.Null(rootNodeA.Storage.GetNodeParent(rootNodeA.Identifier));
		Assert.Equal(FeedNodeType.Group, rootNodeA.Type);
		Assert.Equal("root", rootNodeA.Title);
		Assert.Equal(Symbol.Feed, rootNodeA.Symbol);
		Assert.False(rootNodeA.IsUserCustomizable);
		Assert.NotNull(rootNodeA.Children);
		var childNodeA = Assert.IsAssignableFrom<IReadOnlyStoredFeedNode>(Assert.Single(rootNodeA.Children!));
		Assert.Equal(childNodeA, childNodeA.Storage.GetNode(childNodeA.Identifier));
		Assert.Equal(rootNodeA, childNodeA.Storage.GetNodeParent(childNodeA.Identifier));
		Assert.Equal(FeedNodeType.Group, childNodeA.Type);
		Assert.Equal("child", childNodeA.Title);
		Assert.Equal(Symbol.Directory, childNodeA.Symbol);
		Assert.True(childNodeA.IsUserCustomizable);
		Assert.NotNull(childNodeA.Children);
		var feedNodeA = Assert.IsAssignableFrom<IReadOnlyStoredFeedNode>(Assert.Single(childNodeA.Children!));
		Assert.Equal(feedNodeA, feedNodeA.Storage.GetNode(feedNodeA.Identifier));
		Assert.Equal(childNodeA, feedNodeA.Storage.GetNodeParent(feedNodeA.Identifier));
		Assert.Equal(FeedNodeType.Custom, feedNodeA.Type);
		Assert.Equal("feed", feedNodeA.Title);
		Assert.Equal(Symbol.Web, feedNodeA.Symbol);
		Assert.True(feedNodeA.IsUserCustomizable);
		Assert.Null(feedNodeA.Children);
		var feedA = Assert.IsType<FeedMock>(feedNodeA.Feed);
		Assert.Equal(feedIdentifier, feedA.Identifier);
		var overviewChildA = Assert.Single(Assert.IsAssignableFrom<CompositeFeed>(serviceA.OverviewNode.Feed).Feeds);
		Assert.Equal(rootNodeA.Feed, overviewChildA);
		
		provider.InitialTree = FeedNode.Group(null, null, false);
		var serviceB = new FeedService(DatabaseService, PluginService);
		await serviceB.InitializeAsync();

		var rootNodeB = Assert.Single(serviceB.ProviderNodes);
		Assert.Equal(provider, rootNodeB.Storage.Provider);
		Assert.Equal(rootNodeB, rootNodeB.Storage.GetNode(rootNodeB.Identifier));
		Assert.Null(rootNodeB.Storage.GetNodeParent(rootNodeB.Identifier));
		Assert.Equal(rootNodeA.Identifier, rootNodeB.Identifier);
		Assert.Equal(FeedNodeType.Group, rootNodeB.Type);
		Assert.Equal("root", rootNodeB.Title);
		Assert.Equal(Symbol.Feed, rootNodeB.Symbol);
		Assert.False(rootNodeB.IsUserCustomizable);
		Assert.NotNull(rootNodeB.Children);
		var childNodeB = Assert.IsAssignableFrom<IReadOnlyStoredFeedNode>(Assert.Single(rootNodeB.Children!));
		Assert.Equal(childNodeB, childNodeB.Storage.GetNode(childNodeB.Identifier));
		Assert.Equal(rootNodeB, childNodeB.Storage.GetNodeParent(childNodeB.Identifier));
		Assert.Equal(childNodeA.Identifier, childNodeB.Identifier);
		Assert.Equal(FeedNodeType.Group, childNodeB.Type);
		Assert.Equal("child", childNodeB.Title);
		Assert.Equal(Symbol.Directory, childNodeB.Symbol);
		Assert.True(childNodeB.IsUserCustomizable);
		Assert.NotNull(childNodeB.Children);
		var feedNodeB = Assert.IsAssignableFrom<IReadOnlyStoredFeedNode>(Assert.Single(childNodeB.Children!));
		Assert.Equal(feedNodeB, feedNodeB.Storage.GetNode(feedNodeB.Identifier));
		Assert.Equal(childNodeB, feedNodeB.Storage.GetNodeParent(feedNodeB.Identifier));
		Assert.Equal(feedNodeA.Identifier, feedNodeB.Identifier);
		Assert.Equal(FeedNodeType.Custom, feedNodeB.Type);
		Assert.Equal("feed", feedNodeB.Title);
		Assert.Equal(Symbol.Web, feedNodeB.Symbol);
		Assert.True(feedNodeB.IsUserCustomizable);
		Assert.Null(feedNodeB.Children);
		var feedB = Assert.IsType<FeedMock>(feedNodeB.Feed);
		Assert.Equal(feedIdentifier, feedB.Identifier);
		var overviewChildB = Assert.Single(Assert.IsAssignableFrom<CompositeFeed>(serviceB.OverviewNode.Feed).Feeds);
		Assert.Equal(rootNodeB.Feed, overviewChildB);
	}

	private async Task<IReadOnlyStoredFeedNode> CreateFeedNodeAsync(Guid? identifier = null)
	{
		PluginService.AvailableFeedProviders = ImmutableArray.Create<FeedProvider>(
			new FeedProviderMock(identifier ?? Guid.Parse("ab810ada-234c-46dd-a62d-0d76c61f29f1")));
		var service = new FeedService(DatabaseService, PluginService);
		await service.InitializeAsync();
		return service.ProviderNodes[0];
	}

	[Fact]
	public async Task FeedStorage_ReuseItemStorages()
	{
		var node = await CreateFeedNodeAsync();
		var storageA = node.Storage.GetItemStorage(Guid.Parse("45bdb8dc-c27c-48ce-978c-158d1a78d5bc"));
		var storageB = node.Storage.GetItemStorage(Guid.Parse("45bdb8dc-c27c-48ce-978c-158d1a78d5bc"));
		var storageC = node.Storage.GetItemStorage(Guid.Parse("5956070c-39d3-4e69-bd30-6199b6399d6d"));
		Assert.Equal(storageA, storageB);
		Assert.NotEqual(storageA, storageC);
	}

	[Fact]
	public async Task FeedStorage_AddChildNode()
	{
		var rootNodeA = await CreateFeedNodeAsync();
		var feedA = new FeedMock(Guid.Parse("c71e97f0-5b25-43db-908c-8e8d0b4bbe66"));
		var nodeA = await rootNodeA.Storage.AddNodeAsync(
			FeedNode.Custom(feedA, "feed", Symbol.Web, true), rootNodeA.Identifier);
		Assert.Collection(
			rootNodeA.Children!,
			node => Assert.Equal(nodeA, node));
		Assert.Equal(nodeA, nodeA.Storage.GetNode(nodeA.Identifier));
		Assert.Equal(rootNodeA, nodeA.Storage.GetNodeParent(nodeA.Identifier));
		Assert.Equal(FeedNodeType.Custom, nodeA.Type);
		Assert.Equal("feed", nodeA.Title);
		Assert.Equal(Symbol.Web, nodeA.Symbol);
		Assert.Equal(feedA, nodeA.Feed);
		Assert.True(nodeA.IsUserCustomizable);
		Assert.Null(nodeA.Children);

		var rootNodeB = await CreateFeedNodeAsync();
		var nodeB = Assert.IsAssignableFrom<IReadOnlyStoredFeedNode>(Assert.Single(rootNodeB.Children!));
		var feedB = Assert.IsType<FeedMock>(nodeB.Feed);
		Assert.Equal(nodeA.Identifier, nodeB.Identifier);
		Assert.Equal(nodeB, nodeB.Storage.GetNode(nodeB.Identifier));
		Assert.Equal(rootNodeB, nodeB.Storage.GetNodeParent(nodeB.Identifier));
		Assert.Equal(FeedNodeType.Custom, nodeB.Type);
		Assert.Equal("feed", nodeB.Title);
		Assert.Equal(Symbol.Web, nodeB.Symbol);
		Assert.Equal(feedA.Identifier, feedB.Identifier);
		Assert.True(nodeB.IsUserCustomizable);
		Assert.Null(nodeB.Children);
	}

	[Fact]
	public async Task FeedStorage_AddChildNode_Sorted()
	{
		var rootNode = await CreateFeedNodeAsync();
		await rootNode.Storage.AddNodeAsync(FeedNode.Group("baz", Symbol.Web, true), rootNode.Identifier);
		await rootNode.Storage.AddNodeAsync(FeedNode.Group("bar", Symbol.Web, true), rootNode.Identifier);
		await rootNode.Storage.AddNodeAsync(FeedNode.Group("foo", Symbol.Web, true), rootNode.Identifier);
		Assert.Collection(
			rootNode.Children!,
			node => Assert.Equal("bar", node.Title),
			node => Assert.Equal("baz", node.Title),
			node => Assert.Equal("foo", node.Title));
	}

	[Fact]
	public async Task FeedStorage_RenameNode()
	{
		var rootNodeA = await CreateFeedNodeAsync();
		var nodeA = await rootNodeA.Storage.AddNodeAsync(
			FeedNode.Group("feed", Symbol.Web, true), rootNodeA.Identifier);
		await nodeA.Storage.RenameNodeAsync(nodeA.Identifier, "updated");
		Assert.Equal("updated", nodeA.Title);

		var rootNodeB = await CreateFeedNodeAsync();
		var nodeB = Assert.IsAssignableFrom<IReadOnlyStoredFeedNode>(Assert.Single(rootNodeB.Children!));
		Assert.Equal("updated", nodeB.Title);
	}

	[Fact]
	public async Task FeedStorage_MoveNode_SameParent()
	{
		var rootNodeA = await CreateFeedNodeAsync();
		var parentA = await rootNodeA.Storage.AddNodeAsync(
			FeedNode.Group("parent", Symbol.Web, true), rootNodeA.Identifier);
		var childA = await rootNodeA.Storage.AddNodeAsync(
			FeedNode.Group("child", Symbol.Web, true), parentA.Identifier);
		await childA.Storage.MoveNodeAsync(childA.Identifier, parentA.Identifier);
		Assert.Collection(
			parentA.Children!,
			node => Assert.Equal(childA, node));
		Assert.Equal(parentA, childA.Storage.GetNodeParent(childA.Identifier));

		var rootNodeB = await CreateFeedNodeAsync();
		var parentB = Assert.IsAssignableFrom<IReadOnlyStoredFeedNode>(Assert.Single(rootNodeB.Children!));
		var childB = Assert.IsAssignableFrom<IReadOnlyStoredFeedNode>(Assert.Single(parentB.Children!));
		Assert.Equal(parentB, childB.Storage.GetNodeParent(childB.Identifier));
	}

	[Fact]
	public async Task FeedStorage_MoveNode_DifferentParent()
	{
		var rootNodeA = await CreateFeedNodeAsync();
		var parentA = await rootNodeA.Storage.AddNodeAsync(
			FeedNode.Group("parent", Symbol.Web, true), rootNodeA.Identifier);
		var childA = await rootNodeA.Storage.AddNodeAsync(
			FeedNode.Group("child", Symbol.Web, true), rootNodeA.Identifier);
		await childA.Storage.MoveNodeAsync(childA.Identifier, parentA.Identifier);
		Assert.Collection(
			rootNodeA.Children!,
			node => Assert.Equal(parentA, node));
		Assert.Collection(
			parentA.Children!,
			node => Assert.Equal(childA, node));
		Assert.Equal(parentA, childA.Storage.GetNodeParent(childA.Identifier));

		var rootNodeB = await CreateFeedNodeAsync();
		var parentB = Assert.IsAssignableFrom<IReadOnlyStoredFeedNode>(Assert.Single(rootNodeB.Children!));
		var childB = Assert.IsAssignableFrom<IReadOnlyStoredFeedNode>(Assert.Single(parentB.Children!));
		Assert.Equal(parentB, childB.Storage.GetNodeParent(childB.Identifier));
	}

	[Fact]
	public async Task FeedStorage_DeleteNode()
	{
		var rootNodeA = await CreateFeedNodeAsync();
		var parentA = await rootNodeA.Storage.AddNodeAsync(
			FeedNode.Group("parent", Symbol.Web, true), rootNodeA.Identifier);
		var childA = await rootNodeA.Storage.AddNodeAsync(
			FeedNode.Group("child", Symbol.Web, true), parentA.Identifier);
		var deletedArgs = await Assert.RaisesAsync<FeedNodesDeletedEventArgs>(
			h => rootNodeA.Storage.NodesDeleted += h, h => rootNodeA.Storage.NodesDeleted -= h,
			() => rootNodeA.Storage.DeleteNodeAsync(parentA.Identifier));
		Assert.Collection(
			deletedArgs.Arguments.Nodes,
			node => Assert.Equal(parentA, node),
			node => Assert.Equal(childA, node));
		Assert.Empty(rootNodeA.Children!);
		Assert.Null(rootNodeA.Storage.GetNode(parentA.Identifier));
		Assert.Null(rootNodeA.Storage.GetNode(childA.Identifier));
		
		var rootNodeB = await CreateFeedNodeAsync();
		Assert.Empty(rootNodeB.Children!);
		Assert.Null(rootNodeB.Storage.GetNode(parentA.Identifier));
		Assert.Null(rootNodeB.Storage.GetNode(childA.Identifier));
	}

	private async Task<IItemStorage> CreateItemStorageAsync(Guid? identifier = null)
	{
		var node = await CreateFeedNodeAsync();
		return node.Storage.GetItemStorage(identifier ?? Guid.Parse("bc9a3db2-8657-478b-9abe-93a1b34d0459"));
	}

	private static Item CreateDummyItem()
	{
		var content = new ArticleItemContent(new RichText(new GenericBlock(new TextInline("Content"))));
		return new Item(
			new Uri("https://www.example.com/"), null, DateTimeOffset.Now - TimeSpan.FromDays(2), 
			DateTimeOffset.Now - TimeSpan.FromDays(1), "Title", "Author", "Summary",
			new StaticItemContentLoader(content));
	}

	private async Task EnsureDatabaseUpToDateAsync(IReadOnlyStoredItem original, IReadOnlyStoredItem loaded)
	{
		Assert.Equal(original.Identifier, loaded.Identifier);
		Assert.Equal(original.Url, loaded.Url);
		Assert.Equal(original.ContentUrl, loaded.ContentUrl);
		Assert.Equal(original.PublishedTimestamp, loaded.PublishedTimestamp);
		Assert.Equal(original.ModifiedTimestamp, loaded.ModifiedTimestamp);
		Assert.Equal(original.Title, loaded.Title);
		Assert.Equal(original.Author, loaded.Author);
		Assert.Equal(await original.LoadContentAsync(), await loaded.LoadContentAsync());
	}

	[Fact]
	public async Task ItemStorage_StoreNewItems()
	{
		var itemStorage = await CreateItemStorageAsync();
		var item = CreateDummyItem();
		var storedItem = Assert.Single(await itemStorage.AddItemsAsync(new[] { item }));
		Assert.Equal(item.Url, storedItem.Url);
		Assert.Equal(item.ContentUrl, storedItem.ContentUrl);
		Assert.Equal(item.PublishedTimestamp, storedItem.PublishedTimestamp);
		Assert.Equal(item.ModifiedTimestamp, storedItem.ModifiedTimestamp);
		Assert.Equal(item.Title, storedItem.Title);
		Assert.Equal(item.Author, storedItem.Author);
		Assert.Equal(await item.LoadContentAsync(), await storedItem.LoadContentAsync());
		var newItemStorage = await CreateItemStorageAsync();
		var newStoredItem = Assert.Single(await newItemStorage.GetItemsAsync());
		await EnsureDatabaseUpToDateAsync(storedItem, newStoredItem);
	}

	[Fact]
	public async Task ItemStorage_UpdateItems_NotModified()
	{
		var itemStorage = await CreateItemStorageAsync();
		var item = CreateDummyItem();
		var storedItem = Assert.Single(await itemStorage.AddItemsAsync(new[] { item }));
		var updatedContent = new ArticleItemContent(new RichText(new GenericBlock(new TextInline("Updated Content"))));
		var updatedItem =
			new Item(item)
			{
				ContentUrl = new Uri("https://foo"),
				PublishedTimestamp = DateTimeOffset.Now,
				Title = "Updated Title",
				Author = "Updated Author",
				Summary = "Updated Summary",
				ContentLoader = new StaticItemContentLoader(updatedContent)
			};
		var updatedStoredItem = Assert.Single(await itemStorage.AddItemsAsync(new[] { updatedItem }));
		Assert.Equal(storedItem, updatedStoredItem);
		Assert.Equal(item.ContentUrl, updatedStoredItem.ContentUrl);
		Assert.Equal(item.PublishedTimestamp, updatedStoredItem.PublishedTimestamp);
		Assert.Equal(item.ModifiedTimestamp, updatedStoredItem.ModifiedTimestamp);
		Assert.Equal(item.Title, updatedStoredItem.Title);
		Assert.Equal(item.Author, updatedStoredItem.Author);
		Assert.Equal(await item.LoadContentAsync(), await updatedStoredItem.LoadContentAsync());
		var newItemStorage = await CreateItemStorageAsync();
		var newStoredItem = Assert.Single(await newItemStorage.GetItemsAsync());
		await EnsureDatabaseUpToDateAsync(updatedStoredItem, newStoredItem);
	}
	
	[Fact]
	public async Task ItemStorage_UpdateItems_Modified()
	{
		var itemStorage = await CreateItemStorageAsync();
		var item = CreateDummyItem();
		var storedItem = Assert.Single(await itemStorage.AddItemsAsync(new[] { item }));
		var updatedContent = new ArticleItemContent(new RichText(new GenericBlock(new TextInline("Updated Content"))));
		var updatedItem =
			new Item(item)
			{
				ContentUrl = new Uri("https://foo"),
				PublishedTimestamp = DateTimeOffset.Now - TimeSpan.FromDays(1),
				ModifiedTimestamp = DateTimeOffset.Now,
				Title = "Updated Title",
				Author = "Updated Author",
				Summary = "Updated Summary",
				ContentLoader = new StaticItemContentLoader(updatedContent)
			};
		var updatedStoredItem = Assert.Single(await itemStorage.AddItemsAsync(new[] { updatedItem }));
		Assert.Equal(storedItem, updatedStoredItem);
		Assert.Equal(updatedItem.ContentUrl, updatedStoredItem.ContentUrl);
		Assert.Equal(item.PublishedTimestamp, updatedStoredItem.PublishedTimestamp);
		Assert.Equal(updatedItem.ModifiedTimestamp, updatedStoredItem.ModifiedTimestamp);
		Assert.Equal(updatedItem.Title, updatedStoredItem.Title);
		Assert.Equal(updatedItem.Author, updatedStoredItem.Author);
		Assert.Equal(await updatedItem.LoadContentAsync(), await updatedStoredItem.LoadContentAsync());
		var newItemStorage = await CreateItemStorageAsync();
		var newStoredItem = Assert.Single(await newItemStorage.GetItemsAsync());
		await EnsureDatabaseUpToDateAsync(updatedStoredItem, newStoredItem);
	}

	[Fact]
	public async Task ItemStorage_OnlyLoadItemsForStorageIdentifier()
	{
		var itemStorageA = await CreateItemStorageAsync();
		var item = CreateDummyItem();
		await itemStorageA.AddItemsAsync(new[] { item });
		var itemStorageB = await CreateItemStorageAsync(Guid.Parse("6f778c4d-4969-476f-8e36-413d8f5e7a20"));
		Assert.Empty(await itemStorageB.GetItemsAsync());
	}

	[Fact]
	public async Task ItemStorage_SetRead()
	{
		var itemStorage = await CreateItemStorageAsync();
		var item = CreateDummyItem();
		var storedItem = Assert.Single(await itemStorage.AddItemsAsync(new[] { item }));
		var updatedItem = await itemStorage.SetItemReadAsync(storedItem.Identifier, isRead: true);
		Assert.Equal(storedItem, updatedItem);
		Assert.True(storedItem.IsRead);
		var newItemStorage = await CreateItemStorageAsync();
		var newStoredItem = Assert.Single(await newItemStorage.GetItemsAsync());
		Assert.True(newStoredItem.IsRead);
	}

	[Fact]
	public async Task ItemStorage_Delete()
	{
		var itemStorage = await CreateItemStorageAsync();
		var item = CreateDummyItem();
		var storedItem = Assert.Single(await itemStorage.AddItemsAsync(new[] { item }));
		var deletedArgs = await Assert.RaisesAsync<ItemsDeletedEventArgs>(
			h => itemStorage.ItemsDeleted += h, h => itemStorage.ItemsDeleted -= h,
			() => itemStorage.DeleteItemsAsync(new[] { storedItem.Identifier }));
		Assert.Collection(
			deletedArgs.Arguments.Items,
			deleted => Assert.Equal(storedItem, deleted));
		Assert.Empty(await itemStorage.GetItemsAsync());
		var newItemStorage = await CreateItemStorageAsync();
		Assert.Empty(await newItemStorage.GetItemsAsync());
	}
}
