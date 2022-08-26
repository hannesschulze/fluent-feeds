using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentFeeds.Feeds.HackerNews.Download;
using FluentFeeds.Feeds.HackerNews.Models;

namespace FluentFeeds.Feeds.HackerNews.Tests.Mock;

public sealed class DownloaderMock : IDownloader
{
	public ItemListResponse? ItemListResponse { get; set; }
	public IReadOnlyDictionary<long, ItemResponse> ItemResponse { get; set; } = new Dictionary<long, ItemResponse>();
	
	public Task<ItemListResponse> DownloadItemListAsync(
		HackerNewsFeedType feedType, CancellationToken cancellation = default)
	{
		var response = ItemListResponse ?? throw new Exception("error");
		return Task.FromResult(response);
	}

	public Task<ItemResponse> DownloadItemAsync(long identifier, CancellationToken cancellation = default)
	{
		var response = ItemResponse.GetValueOrDefault(identifier) ?? throw new Exception("error");
		return Task.FromResult(response);
	}
}
