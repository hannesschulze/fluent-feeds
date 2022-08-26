using System.Collections.Generic;
using FluentFeeds.Feeds.Base;
using FluentFeeds.Feeds.Syndication;

namespace FluentFeeds.App.Shared.Services.Default;

public class PluginService : IPluginService
{
	public IEnumerable<FeedProvider> GetAvailableFeedProviders() =>
		new FeedProvider[]
		{
			//new SyndicationFeedProvider()
			new DummyFeedProvider()
		};
}
