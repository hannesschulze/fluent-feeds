using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using FluentFeeds.Documents;
using FluentFeeds.Feeds.Base.Items;
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

	public static DateTimeOffset ConvertItemTimestamp(ItemResponse item)
	{
		return item.Time != null ? DateTimeOffset.FromUnixTimeSeconds(item.Time.Value) : DateTimeOffset.Now;
	}

	public static string ConvertItemAuthor(ItemResponse item)
	{
		return item.By != null ? $"Hacker News ({item.By})" : "Hacker News";
	}

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
}
