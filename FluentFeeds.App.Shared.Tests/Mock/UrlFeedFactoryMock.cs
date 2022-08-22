using System;
using System.Threading.Tasks;
using FluentFeeds.Feeds.Base;
using FluentFeeds.Feeds.Base.Factories;
using FluentFeeds.Feeds.Base.Storage;

namespace FluentFeeds.App.Shared.Tests.Mock;

public sealed class UrlFeedFactoryMock : IUrlFeedFactory
{
	public Task<Feed> CreateAsync(IFeedStorage feedStorage, Uri url)
	{
		return Task.FromResult<Feed>(new FeedMock(Guid.NewGuid(), url));
	}
}
