using System;
using FluentFeeds.Feeds.Base.Storage;

namespace FluentFeeds.App.Shared.Tests.Services.Mock;

public sealed class FeedStorageMock : IFeedStorage
{
	public IItemStorage GetItemStorage(Guid identifier, IItemContentSerializer? contentSerializer = null) =>
		new ItemStorageMock(identifier);
}
