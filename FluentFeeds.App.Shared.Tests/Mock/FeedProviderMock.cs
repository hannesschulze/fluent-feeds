using System;
using System.Threading.Tasks;
using FluentFeeds.App.Shared.Models.Feeds;
using FluentFeeds.App.Shared.Models.Storage;
using FluentFeeds.Common;
using FluentFeeds.Feeds.Base;
using FluentFeeds.Feeds.Base.Feeds;
using FluentFeeds.Feeds.Base.Feeds.Content;

namespace FluentFeeds.App.Shared.Tests.Mock;

public sealed class FeedProviderMock : FeedProvider
{
	public FeedProviderMock(Guid identifier, bool hasUrlFactory = true) : base(
		new FeedProviderMetadata(identifier, "Feed Provider Mock", "Feed provider mock implementation."))
	{
		InitialTree = new GroupFeedDescriptor("Feed Provider Mock", Symbol.Directory);
		if (hasUrlFactory)
		{
			UrlFeedFactory = new UrlFeedFactoryMock();
		}
	}

	public GroupFeedDescriptor InitialTree { get; set; }

	public override GroupFeedDescriptor CreateInitialTree()
	{
		return InitialTree;
	}

	public override Task<IFeedContentLoader> LoadFeedAsync(string serialized)
	{
		return Task.FromResult<IFeedContentLoader>(new FeedContentLoaderMock(serialized));
	}

	public override Task<string> StoreFeedAsync(IFeedContentLoader feed)
	{
		var testFeed = (FeedContentLoaderMock)feed;
		return Task.FromResult(testFeed.Identifier);
	}
}
