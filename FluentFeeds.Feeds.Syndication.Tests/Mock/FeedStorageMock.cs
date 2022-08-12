using System;
using FluentFeeds.Feeds.Base.Storage;

namespace FluentFeeds.Feeds.Syndication.Tests.Mock;

public sealed class FeedStorageMock : IFeedStorage
{
	public IItemStorage GetItemStorage(Guid identifier) => new ItemStorageMock(identifier);
}
