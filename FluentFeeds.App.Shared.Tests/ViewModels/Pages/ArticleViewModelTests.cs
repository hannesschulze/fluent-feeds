using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using FluentFeeds.App.Shared.Models;
using FluentFeeds.App.Shared.Models.Items;
using FluentFeeds.App.Shared.Models.Navigation;
using FluentFeeds.App.Shared.Tests.Mock;
using FluentFeeds.App.Shared.ViewModels.Pages;
using FluentFeeds.Documents;
using FluentFeeds.Documents.Blocks;
using FluentFeeds.Documents.Inlines;
using FluentFeeds.Feeds.Base.Items;
using FluentFeeds.Feeds.Base.Items.Content;
using Xunit;

namespace FluentFeeds.App.Shared.Tests.ViewModels.Pages;

public class ArticleViewModelTests
{
	public ArticleViewModelTests()
	{
		Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
		Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
	}
	
	private SettingsServiceMock SettingsService { get; } = new();

	private static IItemView CreateItem(
		string title, string? author, DateTimeOffset publishedTimestamp, ArticleItemContent content)
	{
		var storage = new ItemStorageMock();
		return storage.AddItems(
			new[]
			{
				new ItemDescriptor(
					identifier: null, title: title, author: author, summary: null, 
					publishedTimestamp: publishedTimestamp, modifiedTimestamp: DateTimeOffset.Now,
					url: null, contentUrl: null, contentLoader: new StaticItemContentLoader(content))
			}, Guid.Empty).First();
	}

	[Theory]
	[InlineData(null, "Published on Tuesday, 23 August 2022 22:33")]
	[InlineData("author", "Published by author on Tuesday, 23 August 2022 22:33")]
	public void LoadArticle(string? author, string expectedItemInfo)
	{
		var content = new ArticleItemContent(new RichText(new GenericBlock(new TextInline("content"))));
		var item = CreateItem("title", author, new DateTime(2022, 8, 23, 22, 33, 15, DateTimeKind.Local), content);
		var viewModel = new ArticleViewModel(SettingsService);
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
		var viewModel = new ArticleViewModel(SettingsService);
		viewModel.Load(FeedNavigationRoute.Article(item, content));
		((Item)item).Title = "updated title";
		((Item)item).Author = "updated author";
		Assert.Equal("updated title", viewModel.Title);
		Assert.Equal("Published by updated author on Tuesday, 23 August 2022 22:33", viewModel.ItemInfo);
		Assert.Equal(content.Body, viewModel.Content);
	}

	[Fact]
	public void UpdateDisplaySettings()
	{
		SettingsService.ContentFontFamily = FontFamily.Serif;
		SettingsService.ContentFontSize = FontSize.Small;
		var viewModel = new ArticleViewModel(SettingsService);
		Assert.Equal(FontFamily.Serif, viewModel.FontFamily);
		Assert.Equal(FontSize.Small, viewModel.FontSize);
		SettingsService.ContentFontFamily = FontFamily.Monospace;
		Assert.Equal(FontFamily.Monospace, viewModel.FontFamily);
		SettingsService.ContentFontSize = FontSize.Large;
		Assert.Equal(FontSize.Large, viewModel.FontSize);
	}
}
