using System;
using System.Text.Json;
using System.Threading.Tasks;
using FluentFeeds.Common;
using FluentFeeds.Feeds.Base;
using FluentFeeds.Feeds.Base.Feeds;
using FluentFeeds.Feeds.Base.Feeds.Content;
using FluentFeeds.Feeds.Base.Items.Content;
using FluentFeeds.Feeds.HackerNews.Download;

namespace FluentFeeds.Feeds.HackerNews;

/// <summary>
/// Feed provider implementation fetching feeds using the public Hacker News API.
/// </summary>
public class HackerNewsFeedProvider : FeedProvider
{
	private record FeedDescription(HackerNewsFeedType Type);
	private record ItemDescription(long Identifier);
	
	public HackerNewsFeedProvider()
		: base(new FeedProviderMetadata(
			Identifier: Guid.Parse("9a9d8c17-c940-4069-999a-0016bfdddb11"),
			Name: "Hacker News Feed Provider",
			Description: "A feed provider fetching feeds using the public Hacker News API."))
	{
	}

	public override GroupFeedDescriptor CreateInitialTree()
	{
		// Use a single shared item cache.
		var itemCacheIdentifier = Guid.Parse("7c5f88d5-0fc5-42b9-8979-1ebee066909a");
		return new GroupFeedDescriptor(
			name: "Hacker News", symbol: Symbol.HackerNews,
			new CachedFeedDescriptor(new HackerNewsFeedContentLoader(new Downloader(), HackerNewsFeedType.Top))
			{
				Name = "Top",
				Symbol = Symbol.Trending,
				ItemCacheIdentifier = itemCacheIdentifier,
				IsUserCustomizable = false
			},
			new CachedFeedDescriptor(new HackerNewsFeedContentLoader(new Downloader(), HackerNewsFeedType.Ask))
			{
				Name = "Ask",
				Symbol = Symbol.Question,
				ItemCacheIdentifier = itemCacheIdentifier,
				IsUserCustomizable = false,
				IsExcludedFromGroup = true
			},
			new CachedFeedDescriptor(new HackerNewsFeedContentLoader(new Downloader(), HackerNewsFeedType.Show))
			{
				Name = "Show",
				Symbol = Symbol.Presentation,
				ItemCacheIdentifier = itemCacheIdentifier,
				IsUserCustomizable = false,
				IsExcludedFromGroup = true
			},
			new CachedFeedDescriptor(new HackerNewsFeedContentLoader(new Downloader(), HackerNewsFeedType.Jobs))
			{
				Name = "Jobs",
				Symbol = Symbol.Briefcase,
				ItemCacheIdentifier = itemCacheIdentifier,
				IsUserCustomizable = false,
				IsExcludedFromGroup = true
			}) { IsUserCustomizable = false };
	}

	public override Task<string> StoreFeedAsync(IFeedContentLoader contentLoader)
	{
		var hnLoader = (HackerNewsFeedContentLoader)contentLoader;
		var description = new FeedDescription(Type: hnLoader.FeedType);
		return Task.FromResult(JsonSerializer.Serialize(description));
	}

	public override Task<IFeedContentLoader> LoadFeedAsync(string serialized)
	{
		var description = JsonSerializer.Deserialize<FeedDescription>(serialized) ?? throw new JsonException();
		var downloader = new Downloader();
		return Task.FromResult<IFeedContentLoader>(new HackerNewsFeedContentLoader(downloader, description.Type));
	}

	public override Task<string> StoreItemContentAsync(IItemContentLoader contentLoader)
	{
		var hnLoader = (HackerNewsItemContentLoader)contentLoader;
		var description = new ItemDescription(Identifier: hnLoader.Identifier);
		return Task.FromResult(JsonSerializer.Serialize(description));
	}

	public override Task<IItemContentLoader> LoadItemContentAsync(string serialized)
	{
		var description = JsonSerializer.Deserialize<ItemDescription>(serialized) ?? throw new JsonException();
		var downloader = new Downloader();
		return Task.FromResult<IItemContentLoader>(new HackerNewsItemContentLoader(downloader, description.Identifier));
	}
}
