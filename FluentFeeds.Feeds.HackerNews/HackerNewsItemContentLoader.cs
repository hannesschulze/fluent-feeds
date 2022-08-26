using System;
using System.Collections.Generic;
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
	
	public Task<ItemContent> LoadAsync(bool reload = false, CancellationToken cancellation = default)
	{
		if (_currentRequest == null || reload)
		{
			_currentRequest = LoadAsyncCore();
		}

		return _currentRequest;
	}

	private async ValueTask<ItemContent> LoadFastAsync()
	{
		var response = await Downloader.DownloadItemCommentsAsync(Identifier);
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

	private async ValueTask<ItemContent> LoadSlowAsync()
	{
		var mainResponse = await Downloader.DownloadItemAsync(Identifier);
		var commentIdentifiers = mainResponse.Kids ?? ImmutableArray<long>.Empty;

		var comments = ImmutableArray.CreateBuilder<Comment>(commentIdentifiers.Length);
		foreach (var identifier in commentIdentifiers)
		{
			var comment = await Downloader.DownloadItemAsync(identifier);
			var converted = await ConversionHelpers.ConvertItemCommentAsync(Downloader, comment);
			if (converted != null)
			{
				comments.Add(converted);
			}
		}

		return new CommentItemContent { Comments = comments.ToImmutable(), IsReloadable = true };
	}

	private async Task<ItemContent> LoadAsyncCore()
	{
		try
		{
			// Try to use the Algolia API
			return await LoadFastAsync();
		}
		catch (Exception e)
		{
			// Fall back to the official Hacker News API
			return await LoadSlowAsync();
		}
	}

	private Task<ItemContent>? _currentRequest;
}
