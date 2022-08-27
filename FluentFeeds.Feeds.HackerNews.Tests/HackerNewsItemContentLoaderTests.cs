using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using FluentFeeds.Documents;
using FluentFeeds.Documents.Blocks;
using FluentFeeds.Documents.Inlines;
using FluentFeeds.Feeds.Base.Items.Content;
using FluentFeeds.Feeds.HackerNews.Models;
using FluentFeeds.Feeds.HackerNews.Tests.Mock;
using Xunit;

namespace FluentFeeds.Feeds.HackerNews.Tests;

public class HackerNewsItemContentLoaderTests
{
	
	public HackerNewsItemContentLoaderTests()
	{
		Downloader =
			new DownloaderMock
			{
				ItemCommentsResponse =
					new ItemCommentsResponse(-1)
					{
						Children = ImmutableArray.Create(
							new ItemCommentsResponse(-2) { Author = "foo", Text = "comment a" },
							new ItemCommentsResponse(-3) { Author = "bar", Text = "comment b" })
					},
				ItemResponse =
					new Dictionary<long, ItemResponse>
					{
						[-1] = new(-1, "story") { Title = "story", Kids = ImmutableArray.Create<long>(-2, -3) },
						[-2] = new(-2, "comment") { By = "foo", Text = "slow comment a" },
						[-3] = new(-3, "comment") { By = "bar", Text = "slow comment b" }
					}
			};
		ContentLoader = new HackerNewsItemContentLoader(Downloader, -1);
	}
	
	private DownloaderMock Downloader { get; }
	private HackerNewsItemContentLoader ContentLoader { get; }
	
	[Fact]
	public async Task Load()
	{
		var content = Assert.IsType<CommentItemContent>(await ContentLoader.LoadAsync());
		Assert.Collection(
			content.Comments,
			item => Assert.Equal(new RichText(new GenericBlock(new TextInline("comment a"))), item.Body),
			item => Assert.Equal(new RichText(new GenericBlock(new TextInline("comment b"))), item.Body));
		Downloader.ItemCommentsResponse =
			new ItemCommentsResponse(-1)
			{
				Children = ImmutableArray.Create(
					new ItemCommentsResponse(-4) { Author = "baz", Text = "updated comment" })
			};
		content = Assert.IsType<CommentItemContent>(await ContentLoader.LoadAsync());
		Assert.Collection(
			content.Comments,
			item => Assert.Equal(new RichText(new GenericBlock(new TextInline("comment a"))), item.Body),
			item => Assert.Equal(new RichText(new GenericBlock(new TextInline("comment b"))), item.Body));
		content = Assert.IsType<CommentItemContent>(await ContentLoader.LoadAsync(reload: true));
		Assert.Collection(
			content.Comments,
			item => Assert.Equal(new RichText(new GenericBlock(new TextInline("updated comment"))), item.Body));
	}

	[Fact]
	public async Task Load_UseSlowApiAsFallback()
	{
		Downloader.ItemCommentsResponse = null;
		var content = Assert.IsType<CommentItemContent>(await ContentLoader.LoadAsync());
		Assert.Collection(
			content.Comments,
			item => Assert.Equal(new RichText(new GenericBlock(new TextInline("slow comment a"))), item.Body),
			item => Assert.Equal(new RichText(new GenericBlock(new TextInline("slow comment b"))), item.Body));
	}
}
