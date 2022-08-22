using System.Collections.Generic;
using System.Collections.Immutable;
using FluentFeeds.App.Shared.Services;
using FluentFeeds.Feeds.Base;

namespace FluentFeeds.App.Shared.Tests.Mock;

public sealed class PluginServiceMock : IPluginService
{
	public ImmutableArray<FeedProvider> AvailableFeedProviders { get; set; } = ImmutableArray<FeedProvider>.Empty;

	public IEnumerable<FeedProvider> GetAvailableFeedProviders() => AvailableFeedProviders;
}
