using System;
using System.Collections.Immutable;
using System.Threading.Tasks;
using FluentFeeds.App.Shared.Services.Default;
using FluentFeeds.App.Shared.Tests.Services.Mock;
using FluentFeeds.Common;
using FluentFeeds.Documents;
using FluentFeeds.Documents.Blocks;
using FluentFeeds.Documents.Inlines;
using FluentFeeds.Feeds.Base;
using FluentFeeds.Feeds.Base.Items;
using FluentFeeds.Feeds.Base.Items.Content;
using FluentFeeds.Feeds.Base.Items.ContentLoaders;
using FluentFeeds.Feeds.Base.Nodes;
using FluentFeeds.Feeds.Base.Storage;
using Xunit;
using Xunit.Abstractions;

namespace FluentFeeds.App.Shared.Tests.Services;

public class FeedServiceTests
{
	private sealed class TestFeed : Feed
	{
		public TestFeed(Guid identifier)
		{
			Identifier = identifier;
		}
		
		public Guid Identifier { get; }
		
		protected override Task DoLoadAsync() => Task.CompletedTask;

		protected override Task DoSynchronizeAsync() => Task.CompletedTask;
	}

	public sealed class TestFeedProvider : FeedProvider
	{
		public TestFeedProvider(Guid identifier) : base(
			new FeedProviderMetadata(identifier, "Test Feed Provider", "A simple test feed provider."))
		{
			InitialTree = FeedNode.Group("Test Feed Provider", Symbol.Directory, true);
		}

		public IReadOnlyFeedNode InitialTree { get; set; }

		public override IReadOnlyFeedNode CreateInitialTree(IFeedStorage feedStorage)
		{
			return InitialTree;
		}

		public override Task<Feed> LoadFeedAsync(IFeedStorage feedStorage, string serialized)
		{
			var identifier = Guid.Parse(serialized);
			return Task.FromResult<Feed>(new TestFeed(identifier));
		}

		public override Task<string> StoreFeedAsync(Feed feed)
		{
			var testFeed = (TestFeed)feed;
			return Task.FromResult(testFeed.Identifier.ToString());
		}
	}

	public FeedServiceTests(ITestOutputHelper testOutputHelper)
	{
		DatabaseService = new DatabaseServiceMock(testOutputHelper);
		PluginService = new PluginServiceMock();
	}
	
	public DatabaseServiceMock DatabaseService { get; }
	public PluginServiceMock PluginService { get; }

	[Fact]
	public async Task Initialization_StoreFeedNodesForProviders()
	{
		var providerIdentifier = Guid.Parse("6ebe7ccf-e1ae-4930-9c6e-97cf61fec7e3");
		var feedIdentifier = Guid.Parse("c71e97f0-5b25-43db-908c-8e8d0b4bbe66");
		var provider =
			new TestFeedProvider(providerIdentifier)
			{
				InitialTree =
					FeedNode.Group("root", Symbol.Feed, isUserCustomizable: false, 
						FeedNode.Group("child", Symbol.Directory, isUserCustomizable: true,
							FeedNode.Custom(new TestFeed(feedIdentifier), "feed", Symbol.Web, true)))
			};
		PluginService.AvailableFeedProviders = ImmutableArray.Create<FeedProvider>(provider);
		var serviceA = new FeedService(DatabaseService, PluginService);
		await serviceA.InitializeAsync();

		var loadedProviderA = Assert.Single(serviceA.FeedProviders);
		Assert.Equal(provider, loadedProviderA.Provider);
		var rootNodeA = loadedProviderA.RootNode;
		Assert.Equal(FeedNodeType.Group, rootNodeA.Type);
		Assert.Equal("root", rootNodeA.Title);
		Assert.Equal(Symbol.Feed, rootNodeA.Symbol);
		Assert.False(rootNodeA.IsUserCustomizable);
		Assert.NotNull(rootNodeA.Children);
		var childNodeA = Assert.IsAssignableFrom<IReadOnlyStoredFeedNode>(Assert.Single(rootNodeA.Children!));
		Assert.Equal(FeedNodeType.Group, childNodeA.Type);
		Assert.Equal("child", childNodeA.Title);
		Assert.Equal(Symbol.Directory, childNodeA.Symbol);
		Assert.True(childNodeA.IsUserCustomizable);
		Assert.NotNull(childNodeA.Children);
		var feedNodeA = Assert.IsAssignableFrom<IReadOnlyStoredFeedNode>(Assert.Single(childNodeA.Children!));
		Assert.Equal(FeedNodeType.Custom, feedNodeA.Type);
		Assert.Equal("feed", feedNodeA.Title);
		Assert.Equal(Symbol.Web, feedNodeA.Symbol);
		Assert.True(feedNodeA.IsUserCustomizable);
		Assert.Null(feedNodeA.Children);
		var feedA = Assert.IsType<TestFeed>(feedNodeA.Feed);
		Assert.Equal(feedIdentifier, feedA.Identifier);
		var overviewChildA = Assert.Single(Assert.IsAssignableFrom<CompositeFeed>(serviceA.OverviewFeed.Feed).Feeds);
		Assert.Equal(rootNodeA.Feed, overviewChildA);
		
		provider.InitialTree = FeedNode.Group(null, null, false);
		var serviceB = new FeedService(DatabaseService, PluginService);
		await serviceB.InitializeAsync();

		var loadedProviderB = Assert.Single(serviceB.FeedProviders);
		Assert.Equal(provider, loadedProviderB.Provider);
		var rootNodeB = loadedProviderB.RootNode;
		Assert.Equal(rootNodeA.Identifier, rootNodeB.Identifier);
		Assert.Equal(FeedNodeType.Group, rootNodeB.Type);
		Assert.Equal("root", rootNodeB.Title);
		Assert.Equal(Symbol.Feed, rootNodeB.Symbol);
		Assert.False(rootNodeB.IsUserCustomizable);
		Assert.NotNull(rootNodeB.Children);
		var childNodeB = Assert.IsAssignableFrom<IReadOnlyStoredFeedNode>(Assert.Single(rootNodeB.Children!));
		Assert.Equal(childNodeA.Identifier, childNodeB.Identifier);
		Assert.Equal(FeedNodeType.Group, childNodeB.Type);
		Assert.Equal("child", childNodeB.Title);
		Assert.Equal(Symbol.Directory, childNodeB.Symbol);
		Assert.True(childNodeB.IsUserCustomizable);
		Assert.NotNull(childNodeB.Children);
		var feedNodeB = Assert.IsAssignableFrom<IReadOnlyStoredFeedNode>(Assert.Single(childNodeB.Children!));
		Assert.Equal(feedNodeA.Identifier, feedNodeB.Identifier);
		Assert.Equal(FeedNodeType.Custom, feedNodeB.Type);
		Assert.Equal("feed", feedNodeB.Title);
		Assert.Equal(Symbol.Web, feedNodeB.Symbol);
		Assert.True(feedNodeB.IsUserCustomizable);
		Assert.Null(feedNodeB.Children);
		var feedB = Assert.IsType<TestFeed>(feedNodeB.Feed);
		Assert.Equal(feedIdentifier, feedB.Identifier);
		var overviewChildB = Assert.Single(Assert.IsAssignableFrom<CompositeFeed>(serviceB.OverviewFeed.Feed).Feeds);
		Assert.Equal(rootNodeB.Feed, overviewChildB);
	}

	private async Task<IFeedStorage> CreateFeedStorageAsync()
	{
		PluginService.AvailableFeedProviders = ImmutableArray.Create<FeedProvider>(new TestFeedProvider(Guid.Empty));
		var service = new FeedService(DatabaseService, PluginService);
		await service.InitializeAsync();
		return service.FeedProviders[0].FeedStorage;
	}

	[Fact]
	public async Task FeedStorage_ReuseItemStorages()
	{
		var feedStorage = await CreateFeedStorageAsync();
		var storageA = feedStorage.GetItemStorage(Guid.Parse("45bdb8dc-c27c-48ce-978c-158d1a78d5bc"));
		var storageB = feedStorage.GetItemStorage(Guid.Parse("45bdb8dc-c27c-48ce-978c-158d1a78d5bc"));
		var storageC = feedStorage.GetItemStorage(Guid.Parse("5956070c-39d3-4e69-bd30-6199b6399d6d"));
		Assert.Equal(storageA, storageB);
		Assert.NotEqual(storageA, storageC);
	}

	private async Task<IItemStorage> CreateItemStorageAsync(Guid? identifier = null)
	{
		var feedStorage = await CreateFeedStorageAsync();
		return feedStorage.GetItemStorage(identifier ?? Guid.Empty);
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
}
