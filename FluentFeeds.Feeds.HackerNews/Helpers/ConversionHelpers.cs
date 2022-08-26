using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using FluentFeeds.Documents;
using FluentFeeds.Documents.Blocks;
using FluentFeeds.Documents.Inlines;
using FluentFeeds.Feeds.Base.Items;
using FluentFeeds.Feeds.Base.Items.Content;
using FluentFeeds.Feeds.HackerNews.Download;
using FluentFeeds.Feeds.HackerNews.Models;

namespace FluentFeeds.Feeds.HackerNews.Helpers;

public static class ConversionHelpers
{
	private const int ItemProcessingMaxDegreeOfParallelism = 3;

	/// <summary>
	/// Process conversions in parallel without blocking the thread pool.
	/// </summary>
	public static async ValueTask<ImmutableArray<TOut>> ParallelTransformAsync<TIn, TOut>(
		IEnumerable<TIn> input, Func<TIn, ValueTask<TOut?>> transformFunc)
	{
		var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = ItemProcessingMaxDegreeOfParallelism };
		var result = new ConcurrentBag<TOut>();
		await Parallel.ForEachAsync(
			input, parallelOptions, async (item, _) =>
			{
				var outItem = await transformFunc(item);
				if (outItem != null)
				{
					result.Add(outItem);
				}
			});
		return result.ToArray().ToImmutableArray();
	}

	/// <summary>
	/// Try to convert a string URL to a Uri object.
	/// </summary>
	public static Uri? ConvertUrl(string url)
	{
		return Uri.TryCreate(url, UriKind.Absolute, out var result) ? result : null;
	}

	/// <summary>
	/// Convert a URL to a string similar to the ones seen on the Hacker News frontpage.
	/// </summary>
	public static string ConvertUrlString(Uri url)
	{
		var host = url.Host;
		if (host.StartsWith("www.") && host.Length > 4)
		{
			host = host[4..];
		}
		
		return host;
	}

	/// <summary>
	/// Extract the summary for an item.
	/// </summary>
	public static async ValueTask<string?> ConvertItemSummaryAsync(ItemResponse item, Uri? contentUrl)
	{
		if (item.Text != null)
		{
			var text = await RichText.ParseHtmlAsync(item.Text);
			return text.ToPlainText();
		}

		if (contentUrl != null)
		{
			return ConvertUrlString(contentUrl);
		}

		return null;
	}

	/// <summary>
	/// Convert a regular item's timestamp to a date time offset.
	/// </summary>
	public static DateTimeOffset ConvertItemTimestamp(ItemResponse item)
	{
		return item.Time != null ? DateTimeOffset.FromUnixTimeSeconds(item.Time.Value) : DateTimeOffset.Now;
	}

	/// <summary>
	/// Convert a comment item's timestamp to a date time offset.
	/// </summary>
	public static DateTimeOffset ConvertItemCommentTimestamp(ItemCommentsResponse comment)
	{
		return comment.CreatedAt != null && DateTimeOffset.TryParse(comment.CreatedAt, out var timestamp)
			? timestamp : DateTimeOffset.Now;
	}

	/// <summary>
	/// Convert the author of an item to a string which can be shown in the main item list.
	/// </summary>
	public static string ConvertItemAuthor(ItemResponse item)
	{
		return item.By != null ? $"{item.By} (Hacker News)" : "Hacker News user";
	}

	public static Task<RichText> ConvertItemTextAsync(ItemResponse item)
	{
		if (item.Text != null)
		{
			return RichText.ParseHtmlAsync(item.Text);
		}

		if (item.Deleted == true)
		{
			return Task.FromResult(new RichText(new GenericBlock(new TextInline("[deleted]"))));
		}

		if (item.Dead == true)
		{
			return Task.FromResult(new RichText(new GenericBlock(new TextInline("[unavailable]"))));
		}

		return Task.FromResult(new RichText());
	}

	/// <summary>
	/// Convert an item response into a standard item descriptor.
	/// </summary>
	public static async ValueTask<ItemDescriptor?> ConvertItemDescriptorAsync(IDownloader downloader, ItemResponse item)
	{
		if (item.Type is not ("story" or "job" or "poll") ||
			item.Title == null || item.Deleted == true || item.Dead == true)
			return null;

		var title = await RichText.ParseHtmlAsync(item.Title);
		var contentUrl = item.Url != null ? ConvertUrl(item.Url) : null;
		var summary = await ConvertItemSummaryAsync(item, contentUrl);
		var timestamp = ConvertItemTimestamp(item);
		var author = ConvertItemAuthor(item);
		
		return new ItemDescriptor(
			identifier: $"{item.Id}",
			title: title.ToPlainText(),
			author: author,
			summary: summary,
			publishedTimestamp: timestamp,
			modifiedTimestamp: timestamp,
			url: new Uri($"https://news.ycombinator.com/item?id={item.Id}"),
			contentUrl: contentUrl,
			contentLoader: new HackerNewsItemContentLoader(downloader, item.Id));
	}

	/// <summary>
	/// Convert an Algolia-style comment entry into a standard comment.
	/// </summary>
	public static async ValueTask<Comment?> ConvertItemCommentAsync(ItemCommentsResponse comment)
	{
		if (comment.Text == null || comment.Author == null)
			return null;
		
		var children = ImmutableArray.CreateBuilder<Comment>(comment.Children?.Length ?? 0);
		if (comment.Children != null)
		{
			foreach (var child in comment.Children.Value)
			{
				var converted = await ConvertItemCommentAsync(child);
				if (converted != null)
				{
					children.Add(converted);
				}
			}
		}
		
		var timestamp = ConvertItemCommentTimestamp(comment);
		var body = await RichText.ParseHtmlAsync(comment.Text);

		return
			new Comment
			{
				Author = comment.Author,
				PublishedTimestamp = timestamp,
				Body = body,
				Children = children.ToImmutable()
			};
	}

	/// <summary>
	/// Load and convert a comment response into a standard comment using the official API.
	/// </summary>
	public static async ValueTask<Comment?> ConvertItemCommentAsync(IDownloader downloader, ItemResponse item)
	{
		if (item.Type != "comment" || item.By == null)
			return null;
		
		var children = ImmutableArray.CreateBuilder<Comment>(item.Kids?.Length ?? 0);
		if (item.Kids != null)
		{
			foreach (var child in item.Kids.Value)
			{
				var comment = await downloader.DownloadItemAsync(child);
				var converted = await ConvertItemCommentAsync(downloader, comment);
				if (converted != null)
				{
					children.Add(converted);
				}
			}
		}
		
		var timestamp = ConvertItemTimestamp(item);
		var body = await ConvertItemTextAsync(item);

		return
			new Comment
			{
				Author = item.By,
				PublishedTimestamp = timestamp,
				Body = body,
				Children = children.ToImmutable()
			};
	}
}
