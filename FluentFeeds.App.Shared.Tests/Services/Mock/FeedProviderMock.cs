using System;
using System.Threading.Tasks;
using FluentFeeds.Common;
using FluentFeeds.Feeds.Base;
using FluentFeeds.Feeds.Base.Nodes;
using FluentFeeds.Feeds.Base.Storage;

namespace FluentFeeds.App.Shared.Tests.Services.Mock;

public sealed class FeedProviderMock : FeedProvider
{
	public FeedProviderMock(Guid identifier) : base(
		new FeedProviderMetadata(identifier, "Feed Provider Mock", "Feed provider mock implementation."))
	{
		InitialTree = FeedNode.Group("Feed Provider Mock", Symbol.Directory, true);
	}

	public IReadOnlyFeedNode InitialTree { get; set; }

	public override IReadOnlyFeedNode CreateInitialTree(IFeedStorage feedStorage)
	{
		return InitialTree;
	}

	public override Task<Feed> LoadFeedAsync(IFeedStorage feedStorage, string serialized)
	{
		var identifier = Guid.Parse(serialized);
		return Task.FromResult<Feed>(new FeedMock(identifier));
	}

	public override Task<string> StoreFeedAsync(Feed feed)
	{
		var testFeed = (FeedMock)feed;
		return Task.FromResult(testFeed.Identifier.ToString());
	}
}