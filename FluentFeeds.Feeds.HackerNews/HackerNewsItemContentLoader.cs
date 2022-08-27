using System;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using FluentFeeds.Feeds.Base.Items.Content;
using FluentFeeds.Feeds.HackerNews.Download;
using FluentFeeds.Feeds.HackerNews.Helpers;
using FluentFeeds.Feeds.HackerNews.Models;

namespace FluentFeeds.Feeds.HackerNews;

/// <summary>
/// Class responsible for loading the comments for a Hacker News item.
/// </summary>
public sealed class HackerNewsItemContentLoader : IItemContentLoader
{
	public HackerNewsItemContentLoader(IDownloader downloader, long identifier)
	{
		Downloader = downloader;
		Identifier = identifier;
	}
	
	/// <summary>
	/// Object used to download the item content.
	/// </summary>
	public IDownloader Downloader { get; }
	
	/// <summary>
	/// The item identifier.
	/// </summary>
	public long Identifier { get; }
	
	public async Task<ItemContent> LoadAsync(bool reload = false, CancellationToken cancellation = default)
	{
		if (_cachedResult != null && !reload)
		{
			return _cachedResult;
		}

		var result = await Task.Run(() => LoadAsyncCore(cancellation), cancellation);
		_cachedResult = result;
		return result;
	}

	/// <summary>
	/// Try to load the item content in a single request using the Algolia API.
	/// </summary>
	private static async ValueTask<ItemContent> LoadFastAsync(ItemCommentsResponse response)
	{
		var topLevelComments = response.Children ?? ImmutableArray<ItemCommentsResponse>.Empty;

		var comments = ImmutableArray.CreateBuilder<Comment>(topLevelComments.Length);
		foreach (var comment in topLevelComments)
		{
			var converted = await ConversionHelpers.ConvertItemCommentAsync(comment);
			if (converted != null)
			{
				comments.Add(converted);
			}
		}

		return new CommentItemContent { Comments = comments.ToImmutable(), IsReloadable = true };
	}

	/// <summary>
	/// Fallback: Load the comments one by one using the official Hacker News API.
	/// </summary>
	private async ValueTask<ItemContent> LoadSlowAsync(
		ItemResponse response, CancellationToken cancellation = default)
	{
		var commentIdentifiers = response.Kids ?? ImmutableArray<long>.Empty;

		var comments = ImmutableArray.CreateBuilder<Comment>(commentIdentifiers.Length);
		foreach (var identifier in commentIdentifiers)
		{
			var comment = await Downloader.DownloadItemAsync(identifier, cancellation);
			var converted = await ConversionHelpers.ConvertItemCommentAsync(Downloader, comment, cancellation);
			if (converted != null)
			{
				comments.Add(converted);
			}
		}

		return new CommentItemContent { Comments = comments.ToImmutable(), IsReloadable = true };
	}

	private async Task<ItemContent> LoadAsyncCore(CancellationToken cancellation = default)
	{
		ItemCommentsResponse commentsResponse;
		try
		{
			// Try to use the Algolia API
			commentsResponse = await Downloader.DownloadItemCommentsAsync(Identifier, cancellation);
		}
		catch (Exception)
		{
			// Fall back to the official Hacker News API
			var itemResponse = await Downloader.DownloadItemAsync(Identifier, cancellation);
			return await LoadSlowAsync(itemResponse, cancellation);
		}

		return await LoadFastAsync(commentsResponse);
	}

	private ItemContent? _cachedResult;
}
