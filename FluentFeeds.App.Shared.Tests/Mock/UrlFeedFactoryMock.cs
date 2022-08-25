using System;
using FluentFeeds.Feeds.Base.Factories;
using FluentFeeds.Feeds.Base.Feeds.Content;

namespace FluentFeeds.App.Shared.Tests.Mock;

public sealed class UrlFeedFactoryMock : IUrlFeedFactory
{
	public IFeedContentLoader Create(Uri url)
	{
		var result = new FeedContentLoaderMock(url.ToString());
		result.CompleteLoad(new FeedContent());
		return result;
	}
}
