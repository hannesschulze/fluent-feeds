using System;
using System.Text.Json;
using System.Threading.Tasks;
using FluentFeeds.Common;
using FluentFeeds.Feeds.Base;
using FluentFeeds.Feeds.Base.Feeds;
using FluentFeeds.Feeds.Base.Feeds.Content;
using FluentFeeds.Feeds.Syndication.Download;
using FluentFeeds.Feeds.Syndication.Resources;

namespace FluentFeeds.Feeds.Syndication;

/// <summary>
/// Feed provider implementation for syndication feeds.
/// </summary>
public sealed class SyndicationFeedProvider : FeedProvider
{
	private record FeedDescription(Uri Url);

	private const string BlogFeedUrl = "https://hannesschulze.github.io/fluent-feeds/blog/feeds/atom.xml";
	private const string BlogFeedName = "Fluent Feeds Blog";

	public SyndicationFeedProvider()
		: base(new FeedProviderMetadata(
			Identifier: Guid.Parse("8d468d92-4b69-41e3-8fac-28c99fc923a2"),
			Name: "Syndication Feed Provider",
			Description: "Feed provider for RSS and Atom feeds."))
	{
		UrlFeedFactory = new SyndicationUrlFeedFactory();
	}

	public override GroupFeedDescriptor CreateInitialTree()
	{
		var blogFeedUrl = new Uri(BlogFeedUrl);
		return new GroupFeedDescriptor(name: LocalizedStrings.GroupName, symbol: Symbol.Feed,
			new CachedFeedDescriptor(new SyndicationFeedContentLoader(new FeedDownloader(blogFeedUrl), blogFeedUrl))
			{
				Name = BlogFeedName,
				Symbol = Symbol.Web
			});
	}

	public override Task<IFeedContentLoader> LoadFeedAsync(string serialized)
	{
		var description = JsonSerializer.Deserialize<FeedDescription>(serialized) ?? throw new JsonException();
		var downloader = new FeedDownloader(description.Url);
		return Task.FromResult<IFeedContentLoader>(new SyndicationFeedContentLoader(downloader, description.Url));
	}

	public override Task<string> StoreFeedAsync(IFeedContentLoader feed)
	{
		var syndicationFeed = (SyndicationFeedContentLoader)feed;
		var description = new FeedDescription(Url: syndicationFeed.Url);
		return Task.FromResult(JsonSerializer.Serialize(description));
	}
}
