using System;
using FluentFeeds.Common;
using FluentFeeds.Feeds.Base;
using FluentFeeds.Feeds.Base.Nodes;
using FluentFeeds.Feeds.Syndication.Download;
using FluentFeeds.Feeds.Syndication.Tests.Mock;
using Xunit;

namespace FluentFeeds.Feeds.Syndication.Tests;

public class SyndicationFeedProviderTests
{
	private SyndicationFeedProvider Provider { get; } = new(new FeedStorageMock());
	
	[Fact]
	public void InitialStructure()
	{
		var structure = Provider.CreateInitialTree();
		Assert.Equal(FeedNodeType.Group, structure.Type);
		Assert.Equal(Symbol.Feed, structure.Symbol);
		Assert.Equal("RSS/Atom feeds", structure.Title);
		Assert.NotNull(structure.Children);
		Assert.Empty(structure.Children!);
		Assert.True(structure.IsUserCustomizable);
	}

	[Fact]
	public void Factories()
	{
		var factory = Assert.IsType<SyndicationUrlFeedFactory>(Provider.UrlFeedFactory);
		Assert.Equal(Provider.Storage, factory.FeedStorage);
	}

	[Fact]
	public void FeedSerialization()
	{
		var identifier = Guid.NewGuid();
		var downloader = new FeedDownloaderMock();
		var storage = Provider.Storage.GetItemStorage(identifier);
		var url = new Uri("https://www.example.com/");
		var metadata = new FeedMetadata("name", "author", "description", Symbol.Web);
		var feed = new SyndicationFeed(downloader, storage, identifier, url, metadata);
		var serialized = Provider.StoreFeed(feed);
		var deserialized = Assert.IsType<SyndicationFeed>(Provider.LoadFeed(serialized));
		var newDownloader = Assert.IsType<FeedDownloader>(deserialized.Downloader);
		Assert.Equal(url, newDownloader.Url);
		Assert.Equal(metadata, deserialized.Metadata);
		Assert.Equal(url, deserialized.Url);
		Assert.Equal(identifier, deserialized.Identifier);
		Assert.Equal(identifier, deserialized.CollectionIdentifier);
		var newStorage = Assert.IsType<ItemStorageMock>(deserialized.Storage);
		Assert.Equal(identifier, newStorage.Identifier);
	}
}