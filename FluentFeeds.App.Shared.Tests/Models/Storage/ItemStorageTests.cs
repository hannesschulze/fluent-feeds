using System;
using System.Threading.Tasks;
using FluentFeeds.App.Shared.EventArgs;
using FluentFeeds.App.Shared.Models.Feeds;
using FluentFeeds.App.Shared.Models.Items;
using FluentFeeds.App.Shared.Models.Storage;
using FluentFeeds.App.Shared.Tests.Mock;
using FluentFeeds.Documents;
using FluentFeeds.Documents.Blocks;
using FluentFeeds.Documents.Inlines;
using FluentFeeds.Feeds.Base.Feeds;
using FluentFeeds.Feeds.Base.Items;
using FluentFeeds.Feeds.Base.Items.Content;
using Xunit;
using Xunit.Abstractions;

namespace FluentFeeds.App.Shared.Tests.Models.Storage;

public class ItemStorageTests
{
	public ItemStorageTests(ITestOutputHelper testOutputHelper)
	{
		DatabaseService = new DatabaseServiceMock(testOutputHelper);
	}
	
	private DatabaseServiceMock DatabaseService { get; }

	private IFeedView FeedA { get; set; } = null!;

	private IFeedView FeedB { get; set; } = null!;

	private async Task<IItemStorage> InitializeAsync(Guid? identifier = null)
	{
		var provider = new FeedProviderMock(Guid.Parse("ab810ada-234c-46dd-a62d-0d76c61f29f1"));
		var storageIdentifier = identifier ?? Guid.Parse("bc9a3db2-8657-478b-9abe-93a1b34d0459");
		provider.InitialTree = new GroupFeedDescriptor(null, null,
			new CachedFeedDescriptor(new FeedContentLoaderMock("")) { ItemCacheIdentifier = storageIdentifier },
			new CachedFeedDescriptor(new FeedContentLoaderMock("")) { ItemCacheIdentifier = storageIdentifier });
		var feedStorage = new FeedStorage(DatabaseService, new ItemContentCache(5), provider);
		await DatabaseService.ExecuteAsync(
			async database =>
			{
				var root = await feedStorage.InitializeAsync(database);
				FeedA ??= root.Children![0];
				FeedB ??= root.Children![1];
				await database.SaveChangesAsync();
			});
		return feedStorage.GetItemStorage(storageIdentifier);
	}

	private static async Task EnsureDatabaseUpToDateAsync(IItemView original, IItemView loaded)
	{
		Assert.Equal(original.Identifier, loaded.Identifier);
		Assert.Equal(original.UserIdentifier, loaded.UserIdentifier);
		Assert.Equal(original.Title, loaded.Title);
		Assert.Equal(original.Author, loaded.Author);
		Assert.Equal(original.Summary, loaded.Summary);
		Assert.Equal(original.PublishedTimestamp, loaded.PublishedTimestamp);
		Assert.Equal(original.ModifiedTimestamp, loaded.ModifiedTimestamp);
		Assert.Equal(original.Url, loaded.Url);
		Assert.Equal(original.ContentUrl, loaded.ContentUrl);
		Assert.Equal(await original.LoadContentAsync(), await loaded.LoadContentAsync());
	}

	[Fact]
	public async Task StoreNewItems()
	{
		var itemStorageA = await InitializeAsync();
		var descriptor = new ItemDescriptor(
			identifier: "identifier",
			title: "title",
			author: "author",
			summary: "summary",
			publishedTimestamp: DateTimeOffset.Now - TimeSpan.FromSeconds(2),
			modifiedTimestamp: DateTimeOffset.Now,
			url: new Uri("https://www.example.com"),
			contentUrl: null,
			contentLoader: new StaticItemContentLoader(new ArticleItemContent(new RichText(
				new GenericBlock(new TextInline("content"))))));
		var itemA = Assert.Single(await itemStorageA.AddItemsAsync(new[] { descriptor }, FeedA.Identifier));
		Assert.Equal(descriptor.Identifier, itemA.UserIdentifier);
		Assert.Equal(descriptor.Title, itemA.Title);
		Assert.Equal(descriptor.Author, itemA.Author);
		Assert.Equal(descriptor.Summary, itemA.Summary);
		Assert.Equal(descriptor.PublishedTimestamp, itemA.PublishedTimestamp);
		Assert.Equal(descriptor.ModifiedTimestamp, itemA.ModifiedTimestamp);
		Assert.Equal(descriptor.Url, itemA.Url);
		Assert.Equal(descriptor.ContentUrl, itemA.ContentUrl);
		Assert.Equal(await descriptor.ContentLoader.LoadAsync(), await itemA.LoadContentAsync());
		Assert.Empty(await itemStorageA.GetItemsAsync(FeedB.Identifier));
		
		var itemStorageB = await InitializeAsync();
		var itemB = Assert.Single(await itemStorageB.GetItemsAsync(FeedA.Identifier));
		await EnsureDatabaseUpToDateAsync(itemA, itemB);
		Assert.Empty(await itemStorageB.GetItemsAsync(FeedB.Identifier));
	}

	[Fact]
	public async Task UpdateItems_NotModified()
	{
		var itemStorageA = await InitializeAsync();
		var descriptor = new ItemDescriptor(
			identifier: "identifier",
			title: "title",
			author: "author",
			summary: "summary",
			publishedTimestamp: DateTimeOffset.Now - TimeSpan.FromSeconds(2),
			modifiedTimestamp: DateTimeOffset.Now,
			url: new Uri("https://www.example.com"),
			contentUrl: null,
			contentLoader: new StaticItemContentLoader(new ArticleItemContent(new RichText(
				new GenericBlock(new TextInline("content"))))));
		var itemA = Assert.Single(await itemStorageA.AddItemsAsync(new[] { descriptor }, FeedA.Identifier));
		var updatedDescriptor = new ItemDescriptor(
			identifier: "identifier",
			title: "updated title",
			author: "updated author",
			summary: "updated summary",
			publishedTimestamp: DateTimeOffset.Now - TimeSpan.FromSeconds(2),
			modifiedTimestamp: descriptor.ModifiedTimestamp,
			url: new Uri("https://www.example.com/2"),
			contentUrl: new Uri("https://testurl"),
			contentLoader: new StaticItemContentLoader(new ArticleItemContent(new RichText(
				new GenericBlock(new TextInline("updated content"))))));
		var updatedItem =
			Assert.Single(await itemStorageA.AddItemsAsync(new[] { updatedDescriptor }, FeedB.Identifier));
		Assert.Equal(itemA, updatedItem);
		Assert.Equal(descriptor.Identifier, itemA.UserIdentifier);
		Assert.Equal(descriptor.Title, itemA.Title);
		Assert.Equal(descriptor.Author, itemA.Author);
		Assert.Equal(descriptor.Summary, itemA.Summary);
		Assert.Equal(descriptor.PublishedTimestamp, itemA.PublishedTimestamp);
		Assert.Equal(descriptor.ModifiedTimestamp, itemA.ModifiedTimestamp);
		Assert.Equal(descriptor.Url, itemA.Url);
		Assert.Equal(descriptor.ContentUrl, itemA.ContentUrl);
		Assert.Equal(await descriptor.ContentLoader.LoadAsync(), await itemA.LoadContentAsync());

		var itemStorageB = await InitializeAsync();
		var itemB = Assert.Single(await itemStorageB.GetItemsAsync(FeedA.Identifier));
		await EnsureDatabaseUpToDateAsync(itemA, itemB);
		Assert.Equal(itemB, Assert.Single(await itemStorageB.GetItemsAsync(FeedB.Identifier)));
	}
	
	[Fact]
	public async Task UpdateItems_Modified()
	{
		var itemStorageA = await InitializeAsync();
		var descriptor = new ItemDescriptor(
			identifier: "identifier",
			title: "title",
			author: "author",
			summary: "summary",
			publishedTimestamp: DateTimeOffset.Now - TimeSpan.FromSeconds(2),
			modifiedTimestamp: DateTimeOffset.Now,
			url: new Uri("https://www.example.com"),
			contentUrl: null,
			contentLoader: new StaticItemContentLoader(new ArticleItemContent(new RichText(
				new GenericBlock(new TextInline("content"))))));
		var itemA = Assert.Single(await itemStorageA.AddItemsAsync(new[] { descriptor }, FeedA.Identifier));
		var updatedDescriptor = new ItemDescriptor(
			identifier: "identifier",
			title: "updated title",
			author: "updated author",
			summary: "updated summary",
			publishedTimestamp: DateTimeOffset.Now - TimeSpan.FromSeconds(2),
			modifiedTimestamp: DateTimeOffset.Now + TimeSpan.FromSeconds(2),
			url: new Uri("https://www.example.com/2"),
			contentUrl: new Uri("https://testurl"),
			contentLoader: new StaticItemContentLoader(new ArticleItemContent(new RichText(
				new GenericBlock(new TextInline("updated content"))))));
		var updatedItem =
			Assert.Single(await itemStorageA.AddItemsAsync(new[] { updatedDescriptor }, FeedB.Identifier));
		Assert.Equal(itemA, updatedItem);
		Assert.Equal(descriptor.Identifier, itemA.UserIdentifier);
		Assert.Equal(updatedDescriptor.Title, itemA.Title);
		Assert.Equal(updatedDescriptor.Author, itemA.Author);
		Assert.Equal(updatedDescriptor.Summary, itemA.Summary);
		Assert.Equal(descriptor.PublishedTimestamp, itemA.PublishedTimestamp);
		Assert.Equal(updatedDescriptor.ModifiedTimestamp, itemA.ModifiedTimestamp);
		Assert.Equal(updatedDescriptor.Url, itemA.Url);
		Assert.Equal(updatedDescriptor.ContentUrl, itemA.ContentUrl);
		Assert.Equal(await updatedDescriptor.ContentLoader.LoadAsync(), await itemA.LoadContentAsync());

		var itemStorageB = await InitializeAsync();
		var itemB = Assert.Single(await itemStorageB.GetItemsAsync(FeedA.Identifier));
		await EnsureDatabaseUpToDateAsync(itemA, itemB);
		Assert.Equal(itemB, Assert.Single(await itemStorageB.GetItemsAsync(FeedB.Identifier)));
	}

	[Fact]
	public async Task OnlyLoadItemsForStorageIdentifier()
	{
		var itemStorageA = await InitializeAsync();
		var descriptor = new ItemDescriptor(
			identifier: "identifier",
			title: "title",
			author: "author",
			summary: "summary",
			publishedTimestamp: DateTimeOffset.Now - TimeSpan.FromSeconds(2),
			modifiedTimestamp: DateTimeOffset.Now,
			url: new Uri("https://www.example.com"),
			contentUrl: null,
			contentLoader: new StaticItemContentLoader(new ArticleItemContent(new RichText(
				new GenericBlock(new TextInline("content"))))));
		await itemStorageA.AddItemsAsync(new[] { descriptor }, FeedA.Identifier);

		var itemStorageB = FeedA.Storage!.GetItemStorage(Guid.Parse("6f778c4d-4969-476f-8e36-413d8f5e7a20"));
		Assert.Empty(await itemStorageB.GetItemsAsync(FeedA.Identifier));
	}

	[Fact]
	public async Task SetRead()
	{
		var itemStorageA = await InitializeAsync();
		var descriptor = new ItemDescriptor(
			identifier: "identifier",
			title: "title",
			author: "author",
			summary: "summary",
			publishedTimestamp: DateTimeOffset.Now - TimeSpan.FromSeconds(2),
			modifiedTimestamp: DateTimeOffset.Now,
			url: new Uri("https://www.example.com"),
			contentUrl: null,
			contentLoader: new StaticItemContentLoader(new ArticleItemContent(new RichText())));
		var itemA = Assert.Single(await itemStorageA.AddItemsAsync(new[] { descriptor }, FeedA.Identifier));
		var updatedItem = await itemStorageA.SetItemReadAsync(itemA.Identifier, true);
		Assert.Equal(itemA, updatedItem);
		Assert.True(itemA.IsRead);
		
		var itemStorageB = await InitializeAsync();
		var itemB = Assert.Single(await itemStorageB.GetItemsAsync(FeedA.Identifier));
		Assert.True(itemB.IsRead);
	}

	[Fact]
	public async Task DeleteItems()
	{
		var itemStorageA = await InitializeAsync();
		var descriptor = new ItemDescriptor(
			identifier: "identifier",
			title: "title",
			author: "author",
			summary: "summary",
			publishedTimestamp: DateTimeOffset.Now - TimeSpan.FromSeconds(2),
			modifiedTimestamp: DateTimeOffset.Now,
			url: new Uri("https://www.example.com"),
			contentUrl: null,
			contentLoader: new StaticItemContentLoader(new ArticleItemContent(new RichText())));
		var itemA = Assert.Single(await itemStorageA.AddItemsAsync(new[] { descriptor }, FeedA.Identifier));
		var deletedArgs = await Assert.RaisesAsync<ItemsDeletedEventArgs>(
			h => itemStorageA.ItemsDeleted += h, h => itemStorageA.ItemsDeleted -= h,
			() => itemStorageA.DeleteItemsAsync(new[] { itemA.Identifier }));
		Assert.Collection(
			deletedArgs.Arguments.Items,
			deleted => Assert.Equal(itemA, deleted));
		Assert.Empty(await itemStorageA.GetItemsAsync(FeedA.Identifier));
		Assert.Empty(await itemStorageA.AddItemsAsync(new[] { descriptor }, FeedA.Identifier));
		Assert.Empty(await itemStorageA.GetItemsAsync(FeedA.Identifier));
		
		var itemStorageB = await InitializeAsync();
		Assert.Empty(await itemStorageB.GetItemsAsync(FeedA.Identifier));
		Assert.Empty(await itemStorageB.AddItemsAsync(new[] { descriptor }, FeedA.Identifier));
		Assert.Empty(await itemStorageB.GetItemsAsync(FeedA.Identifier));
	}
}
