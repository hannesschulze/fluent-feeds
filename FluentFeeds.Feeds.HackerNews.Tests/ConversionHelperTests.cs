using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using FluentFeeds.Documents;
using FluentFeeds.Documents.Blocks;
using FluentFeeds.Documents.Inlines;
using FluentFeeds.Feeds.Base.Items.Content;
using FluentFeeds.Feeds.HackerNews.Helpers;
using FluentFeeds.Feeds.HackerNews.Models;
using FluentFeeds.Feeds.HackerNews.Tests.Mock;
using Xunit;

namespace FluentFeeds.Feeds.HackerNews.Tests;

public class ConversionHelperTests
{
	[Theory]
	[InlineData("./test", null)]
	[InlineData("https://www.example.com", "https://www.example.com/")]
	public void ConvertUrl(string input, string? expectedOutput)
	{
		Assert.Equal(expectedOutput, ConversionHelpers.ConvertUrl(input)?.ToString());
	}

	[Theory]
	[InlineData("https://www.example.com", "example.com")]
	[InlineData("https://example.com", "example.com")]
	[InlineData("https://github.com/hannesschulze", "github.com")]
	public void ConvertUrlString(string input, string expectedOutput)
	{
		Assert.Equal(expectedOutput, ConversionHelpers.ConvertUrlString(new Uri(input)));
	}

	[Fact]
	public async Task ConvertItemSummary_WithContent()
	{
		var response = new ItemResponse(-1, "story") { Text = "item <strong>summary</item>" };
		Assert.Equal(
			"item summary",
			await ConversionHelpers.ConvertItemSummaryAsync(response, new Uri("https://www.example.com")));
	}

	[Fact]
	public async Task ConvertItemSummary_WithoutContent()
	{
		var response = new ItemResponse(-1, "story");
		Assert.Equal(
			"example.com",
			await ConversionHelpers.ConvertItemSummaryAsync(response, new Uri("https://www.example.com")));
	}

	[Fact]
	public async Task ConvertItemSummary_Unavailable()
	{
		var response = new ItemResponse(-1, "story");
		Assert.Null(await ConversionHelpers.ConvertItemSummaryAsync(response, null));
	}

	[Theory]
	[InlineData("foo", "foo (Hacker News)")]
	[InlineData(null, "Hacker News user")]
	public void ConvertItemAuthor(string input, string expectedOutput)
	{
		var response = new ItemResponse(-1, "story") { By = input };
		Assert.Equal(expectedOutput, ConversionHelpers.ConvertItemAuthor(response));
	}

	[Fact]
	public void ConvertItemTimestamp()
	{
		var response = new ItemResponse(-1, "story") { Time = 1203647620 };
		Assert.Equal(
			new DateTimeOffset(2008, 2, 22, 2, 33, 40, TimeSpan.Zero),
			ConversionHelpers.ConvertItemTimestamp(response));
	}

	[Fact]
	public void ConvertItemCommentTimestamp()
	{
		var response = new ItemCommentsResponse(-1) { CreatedAt = "2022-08-25T22:20:35.000Z" };
		Assert.Equal(
			new DateTimeOffset(2022, 8, 25, 22, 20, 35, TimeSpan.Zero),
			ConversionHelpers.ConvertItemCommentTimestamp(response));
	}

	[Fact]
	public async Task ConvertItemText_Available()
	{
		var response = new ItemResponse(-1, "comment") { Text = "item <strong>content</strong>", Deleted = true };
		var result = await ConversionHelpers.ConvertItemTextAsync(response);
		Assert.Equal(
			new RichText(new GenericBlock(new TextInline("item "), new BoldInline(new TextInline("content")))), result);
	}

	[Fact]
	public async Task ConvertItemText_Unavailable_Deleted()
	{
		var response = new ItemResponse(-1, "comment") { Deleted = true };
		var result = await ConversionHelpers.ConvertItemTextAsync(response);
		Assert.Equal(new RichText(new GenericBlock(new TextInline("[deleted]"))), result);
	}

	[Fact]
	public async Task ConvertItemText_Unavailable_Dead()
	{
		var response = new ItemResponse(-1, "comment") { Dead = true };
		var result = await ConversionHelpers.ConvertItemTextAsync(response);
		Assert.Equal(new RichText(new GenericBlock(new TextInline("[unavailable]"))), result);
	}

	[Fact]
	public async Task ConvertItemText_Unavailable_Empty()
	{
		var response = new ItemResponse(-1, "comment");
		var result = await ConversionHelpers.ConvertItemTextAsync(response);
		Assert.Equal(new RichText(), result);
	}

	[Fact]
	public async Task ConvertItemDescriptor_Skip_InvalidType()
	{
		var response = new ItemResponse(-1, "pollopt") { Title = "option" };
		Assert.Null(await ConversionHelpers.ConvertItemDescriptorAsync(new DownloaderMock(), response));
	}

	[Fact]
	public async Task ConvertItemDescriptor_Skip_MissingTitle()
	{
		var response = new ItemResponse(-1, "story");
		Assert.Null(await ConversionHelpers.ConvertItemDescriptorAsync(new DownloaderMock(), response));
	}

	[Fact]
	public async Task ConvertItemDescriptor_Skip_Deleted()
	{
		var response = new ItemResponse(-1, "story") { Deleted = true };
		Assert.Null(await ConversionHelpers.ConvertItemDescriptorAsync(new DownloaderMock(), response));
	}

	[Fact]
	public async Task ConvertItemDescriptor_Skip_Dead()
	{
		var response = new ItemResponse(-1, "story") { Dead = true };
		Assert.Null(await ConversionHelpers.ConvertItemDescriptorAsync(new DownloaderMock(), response));
	}

	[Fact]
	public async Task ConvertItemDescriptor()
	{
		var response =
			new ItemResponse(8863, "story")
			{
				Title = "My YC app: Dropbox - Throw away your USB drive",
				By = "dhouston",
				Url = "http://www.getdropbox.com/u/2/screencast.html",
				Time = 1175714200
			};
		var result = await ConversionHelpers.ConvertItemDescriptorAsync(new DownloaderMock(), response);
		Assert.Equal("8863", result?.Identifier);
		Assert.Equal("My YC app: Dropbox - Throw away your USB drive", result?.Title);
		Assert.Equal("dhouston (Hacker News)", result?.Author);
		Assert.Equal("getdropbox.com", result?.Summary);
		Assert.Equal(new Uri("https://news.ycombinator.com/item?id=8863"), result?.Url);
		Assert.Equal(new Uri("http://www.getdropbox.com/u/2/screencast.html"), result?.ContentUrl);
		Assert.Equal(new DateTimeOffset(2007, 4, 4, 19, 16, 40, TimeSpan.Zero), result?.PublishedTimestamp);
		Assert.Equal(new DateTimeOffset(2007, 4, 4, 19, 16, 40, TimeSpan.Zero), result?.ModifiedTimestamp);
	}

	[Fact]
	public async Task ConvertItemComment_Fast_Skip_MissingAuthor()
	{
		var response = new ItemCommentsResponse(-1) { Text = "content" };
		Assert.Null(await ConversionHelpers.ConvertItemCommentAsync(response));
	}

	[Fact]
	public async Task ConvertItemComment_Fast()
	{
		var response =
			new ItemCommentsResponse(-1)
			{
				Author = "foo",
				CreatedAt = "2007-04-04T19:16:40.000Z",
				Text = "comment <strong>a</strong>",
				Children = ImmutableArray.Create(
					new ItemCommentsResponse(-2)
					{
						Author = "bar",
						CreatedAt = "2007-04-05T01:09:58.000Z",
						Text = "<strong>comment</strong> b"
					},
					new ItemCommentsResponse(-3)
					{
						Author = "baz",
						CreatedAt = "2007-04-05T19:42:55.000Z",
						Text = "other comment"
					})
			};
		var result = await ConversionHelpers.ConvertItemCommentAsync(response);
		Assert.Equal("foo", result?.Author);
		Assert.Equal(new DateTimeOffset(2007, 4, 4, 19, 16, 40, TimeSpan.Zero), result?.PublishedTimestamp);
		Assert.Equal(
			new RichText(new GenericBlock(new TextInline("comment "), new BoldInline(new TextInline("a")))),
			result?.Body);
		Assert.Collection(
			result?.Children ?? ImmutableArray<Comment>.Empty,
			comment =>
			{
				Assert.Equal("bar", comment.Author);
				Assert.Equal(new DateTimeOffset(2007, 4, 5, 1, 9, 58, TimeSpan.Zero), comment.PublishedTimestamp);
				Assert.Equal(
					new RichText(new GenericBlock(new BoldInline(new TextInline("comment")), new TextInline(" b"))),
					comment.Body);
				Assert.Empty(comment.Children);
			},
			comment =>
			{
				Assert.Equal("baz", comment.Author);
				Assert.Equal(new DateTimeOffset(2007, 4, 5, 19, 42, 55, TimeSpan.Zero), comment.PublishedTimestamp);
				Assert.Equal(new RichText(new GenericBlock(new TextInline("other comment"))), comment.Body);
				Assert.Empty(comment.Children);
			});
	}
	
	[Fact]
	public async Task ConvertItemComment_Slow_Skip_InvalidType()
	{
		var response = new ItemResponse(-1, "story") { By = "foo", Text = "content" };
		Assert.Null(await ConversionHelpers.ConvertItemCommentAsync(new DownloaderMock(), response));
	}
	
	[Fact]
	public async Task ConvertItemComment_Slow_Skip_MissingAuthor()
	{
		var response = new ItemResponse(-1, "comment") { Text = "content" };
		Assert.Null(await ConversionHelpers.ConvertItemCommentAsync(new DownloaderMock(), response));
	}

	[Fact]
	public async Task ConvertItemComment_Slow()
	{
		var downloader =
			new DownloaderMock
			{
				ItemResponse =
					new Dictionary<long, ItemResponse>
					{
						[-2] = new(-2, "comment") { By = "bar", Text = "<strong>comment</strong> b" },
						[-3] = new(-3, "comment") { By = "baz", Text = "other comment" },
					}
			};
		var response =
			new ItemResponse(-1, "comment")
			{
				By = "foo",
				Text = "comment <strong>a</strong>",
				Kids = ImmutableArray.Create<long>(-2, -3)
			};
		var result = await ConversionHelpers.ConvertItemCommentAsync(downloader, response);
		Assert.Equal("foo", result?.Author);
		Assert.Equal(
			new RichText(new GenericBlock(new TextInline("comment "), new BoldInline(new TextInline("a")))),
			result?.Body);
		Assert.Collection(
			result?.Children ?? ImmutableArray<Comment>.Empty,
			comment =>
			{
				Assert.Equal("bar", comment.Author);
				Assert.Equal(
					new RichText(new GenericBlock(new BoldInline(new TextInline("comment")), new TextInline(" b"))),
					comment.Body);
				Assert.Empty(comment.Children);
			},
			comment =>
			{
				Assert.Equal("baz", comment.Author);
				Assert.Equal(new RichText(new GenericBlock(new TextInline("other comment"))), comment.Body);
				Assert.Empty(comment.Children);
			});
	}
}
