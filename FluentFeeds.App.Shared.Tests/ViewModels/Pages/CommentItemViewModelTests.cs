using System;
using System.Globalization;
using System.Threading;
using FluentFeeds.App.Shared.Models;
using FluentFeeds.App.Shared.Models.Navigation;
using FluentFeeds.App.Shared.Tests.Mock;
using FluentFeeds.App.Shared.ViewModels.ListItems.Comments;
using FluentFeeds.App.Shared.ViewModels.Pages;
using FluentFeeds.Documents;
using FluentFeeds.Documents.Blocks;
using FluentFeeds.Documents.Inlines;
using FluentFeeds.Feeds.Base.Items.Content;
using Xunit;

namespace FluentFeeds.App.Shared.Tests.ViewModels.Pages;

public class CommentItemViewModelTests
{
	public CommentItemViewModelTests()
	{
		Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
		Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
		
		BodyA = new RichText(new GenericBlock(new TextInline("comment a")));
		BodyB = new RichText(new GenericBlock(new TextInline("comment b")));
		BodyC = new RichText(new GenericBlock(new TextInline("comment c")));
		Content = new CommentItemContent(
			new Comment("foo", new DateTime(2022, 8, 17, 22, 45, 30, DateTimeKind.Local), BodyA),
			new Comment(null, new DateTime(2022, 8, 18, 18, 50, 45, DateTimeKind.Local), BodyB,
				new Comment("baz", new DateTime(2022, 8, 19, 22, 50, 50, DateTimeKind.Local), BodyC)));
	}
	
	private SettingsServiceMock SettingsService { get; } = new();
	
	private RichText BodyA { get; }
	
	private RichText BodyB { get; }
	
	private RichText BodyC { get; }
	
	private CommentItemContent Content { get; }

	[Fact]
	public void LoadArticle()
	{
		var item = ItemViewModelTests.CreateItem(
			"title", null, new DateTime(2022, 8, 15, 22, 33, 15, DateTimeKind.Local), Content);
		var viewModel = new CommentItemViewModel(SettingsService);
		viewModel.Load(FeedNavigationRoute.CommentItem(item, Content));
		Assert.Collection(
			viewModel.Items,
			listItem =>
			{
				var header = Assert.IsType<CommentHeaderViewModel>(listItem);
				Assert.Equal("title", header.Title);
				Assert.Equal("Published on Monday, 15 August 2022 22:33", header.ItemInfo);
			},
			listItem =>
			{
				var comment = Assert.IsType<CommentViewModel>(listItem);
				Assert.Equal("foo | 08/17/2022", comment.CommentInfo);
				Assert.Equal(BodyA, comment.CommentBody);
				Assert.Equal(FontFamily.SansSerif, comment.FontFamily);
				Assert.Equal(FontSize.Normal, comment.FontSize);
				Assert.Empty(comment.Children);
			},
			listItem =>
			{
				var comment = Assert.IsType<CommentViewModel>(listItem);
				Assert.Equal("08/18/2022", comment.CommentInfo);
				Assert.Equal(BodyB, comment.CommentBody);
				Assert.Equal(FontFamily.SansSerif, comment.FontFamily);
				Assert.Equal(FontSize.Normal, comment.FontSize);
				Assert.Collection(
					comment.Children,
					childListItem =>
					{
						var childComment = Assert.IsType<CommentViewModel>(childListItem);
						Assert.Equal("baz | 08/19/2022", childComment.CommentInfo);
						Assert.Equal(BodyC, childComment.CommentBody);
						Assert.Equal(FontFamily.SansSerif, childComment.FontFamily);
						Assert.Equal(FontSize.Normal, childComment.FontSize);
						Assert.Empty(childComment.Children);
					});
			});
	}

	[Fact]
	public void UpdateDisplaySettings()
	{
		var item = ItemViewModelTests.CreateItem(
			"title", null, new DateTime(2022, 8, 15, 22, 33, 15, DateTimeKind.Local), Content);
		var viewModel = new CommentItemViewModel(SettingsService);
		viewModel.Load(FeedNavigationRoute.CommentItem(item, Content));
		SettingsService.ContentFontFamily = FontFamily.Serif;
		SettingsService.ContentFontSize = FontSize.Small;
		Assert.Collection(
			viewModel.Items,
			_ => { },
			listItem =>
			{
				var comment = Assert.IsType<CommentViewModel>(listItem);
				Assert.Equal(FontFamily.Serif, comment.FontFamily);
				Assert.Equal(FontSize.Small, comment.FontSize);
			},
			listItem =>
			{
				var comment = Assert.IsType<CommentViewModel>(listItem);
				Assert.Equal(FontFamily.Serif, comment.FontFamily);
				Assert.Equal(FontSize.Small, comment.FontSize);
				Assert.Collection(
					comment.Children,
					childListItem =>
					{
						var childComment = Assert.IsType<CommentViewModel>(childListItem);
						Assert.Equal(FontFamily.Serif, childComment.FontFamily);
						Assert.Equal(FontSize.Small, childComment.FontSize);
					});
			});
	}
}
