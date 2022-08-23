using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using FluentFeeds.App.Shared.Models.Navigation;
using FluentFeeds.App.Shared.Tests.Mock;
using FluentFeeds.App.Shared.ViewModels.Pages;
using FluentFeeds.Documents;
using FluentFeeds.Documents.Blocks;
using FluentFeeds.Documents.Inlines;
using FluentFeeds.Feeds.Base.Items;
using FluentFeeds.Feeds.Base.Items.Content;
using FluentFeeds.Feeds.Base.Items.ContentLoaders;
using Xunit;

namespace FluentFeeds.App.Shared.Tests.ViewModels.Pages;

public class ArticleViewModelTests
{
	public ArticleViewModelTests()
	{
		Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
	}

	private static IReadOnlyStoredItem CreateItem(
		string title, string? author, DateTimeOffset publishedTimestamp, ArticleItemContent content)
	{
		var storage = new ItemStorageMock();
		return storage.AddItems(
			new[]
			{
				new Item(
					null, null, publishedTimestamp, DateTimeOffset.Now, title, author, null,
					new StaticItemContentLoader(content))
			}).First();
	}

	[Theory]
	[InlineData(null, "Published on Tuesday, 23 August 2022 22:33")]
	[InlineData("author", "Published by author on Tuesday, 23 August 2022 22:33")]
	public void LoadArticle(string? author, string expectedItemInfo)
	{
		var content = new ArticleItemContent(new RichText(new GenericBlock(new TextInline("content"))));
		var item = CreateItem("title", author, new DateTime(2022, 8, 23, 22, 33, 15, DateTimeKind.Local), content);
		var viewModel = new ArticleViewModel();
		viewModel.Load(FeedNavigationRoute.Article(item, content));
		Assert.Equal("title", viewModel.Title);
		Assert.Equal(expectedItemInfo, viewModel.ItemInfo);
		Assert.Equal(content.Body, viewModel.Content);
	}

	[Fact]
	public void UpdateArticle()
	{
		var content = new ArticleItemContent(new RichText(new GenericBlock(new TextInline("content"))));
		var item = CreateItem("title", "author", new DateTime(2022, 8, 23, 22, 33, 15, DateTimeKind.Local), content);
		var viewModel = new ArticleViewModel();
		viewModel.Load(FeedNavigationRoute.Article(item, content));
		((StoredItem)item).Title = "updated title";
		((StoredItem)item).Author = "updated author";
		Assert.Equal("updated title", viewModel.Title);
		Assert.Equal("Published by updated author on Tuesday, 23 August 2022 22:33", viewModel.ItemInfo);
		Assert.Equal(content.Body, viewModel.Content);
	}
}
