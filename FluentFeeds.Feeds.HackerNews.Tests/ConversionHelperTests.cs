using System;
using System.Threading.Tasks;
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

	[Fact]
	public void ConvertItemTimestamp()
	{
		var response = new ItemResponse(-1, "story") { Time = 1203647620 };
		Assert.Equal(
			new DateTimeOffset(2008, 2, 22, 2, 33, 40, TimeSpan.Zero),
			ConversionHelpers.ConvertItemTimestamp(response));
	}

	[Theory]
	[InlineData("foo", "Hacker News (foo)")]
	[InlineData(null, "Hacker News")]
	public void ConvertItemAuthor(string input, string expectedOutput)
	{
		var response = new ItemResponse(-1, "story") { By = input };
		Assert.Equal(expectedOutput, ConversionHelpers.ConvertItemAuthor(response));
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
		Assert.Equal("Hacker News (dhouston)", result?.Author);
		Assert.Equal("getdropbox.com", result?.Summary);
		Assert.Equal(new Uri("https://news.ycombinator.com/item?id=8863"), result?.Url);
		Assert.Equal(new Uri("http://www.getdropbox.com/u/2/screencast.html"), result?.ContentUrl);
		Assert.Equal(new DateTimeOffset(2007, 4, 4, 19, 16, 40, TimeSpan.Zero), result?.PublishedTimestamp);
		Assert.Equal(new DateTimeOffset(2007, 4, 4, 19, 16, 40, TimeSpan.Zero), result?.ModifiedTimestamp);
	}
}
