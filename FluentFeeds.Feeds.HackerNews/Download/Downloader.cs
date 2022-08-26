using System;
using System.Collections.Immutable;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using FluentFeeds.Feeds.HackerNews.Models;

namespace FluentFeeds.Feeds.HackerNews.Download;

/// <summary>
/// Default <see cref="IDownloader"/> implementation.
/// </summary>
public sealed class Downloader : IDownloader
{
	public Downloader()
	{
		_jsonOptions =
			new JsonSerializerOptions
			{
				DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
				Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
			};
	}
	
	public async Task<ItemListResponse> DownloadItemListAsync(
		HackerNewsFeedType feedType, CancellationToken cancellation = default)
	{
		var response = await _httpClient.GetAsync(
			feedType switch
			{
				HackerNewsFeedType.Top => "https://hacker-news.firebaseio.com/v0/topstories.json",
				HackerNewsFeedType.Ask => "https://hacker-news.firebaseio.com/v0/askstories.json",
				HackerNewsFeedType.Show => "https://hacker-news.firebaseio.com/v0/showstories.json",
				HackerNewsFeedType.Jobs => "https://hacker-news.firebaseio.com/v0/jobstories.json",
				_ => throw new ArgumentOutOfRangeException(nameof(feedType), feedType, null)
			}, cancellation);
		response.EnsureSuccessStatusCode();
		var identifiers = await response.Content.ReadFromJsonAsync<ImmutableArray<long>>(_jsonOptions, cancellation);
		return new ItemListResponse(Identifiers: identifiers);
	}

	public async Task<ItemCommentsResponse> DownloadItemCommentsAsync(
		long identifier, CancellationToken cancellation = default)
	{
		var response = await _httpClient.GetAsync($"https://hn.algolia.com/api/v1/items/{identifier}", cancellation);
		response.EnsureSuccessStatusCode();
		var result = await response.Content.ReadFromJsonAsync<ItemCommentsResponse>(_jsonOptions, cancellation);
		return result ?? throw new JsonException();
	}

	public async Task<ItemResponse> DownloadItemAsync(long identifier, CancellationToken cancellation = default)
	{
		var response = await _httpClient.GetAsync(
			$"https://hacker-news.firebaseio.com/v0/item/{identifier}.json", cancellation);
		response.EnsureSuccessStatusCode();
		var result = await response.Content.ReadFromJsonAsync<ItemResponse>(_jsonOptions, cancellation);
		return result ?? throw new JsonException();
	}

	private readonly JsonSerializerOptions _jsonOptions;
	private readonly HttpClient _httpClient = new();
}
