using System.Collections.Generic;
using FluentFeeds.Feeds.Base;

namespace FluentFeeds.App.Shared.Services;

/// <summary>
/// Service managing the list of available feed providers.
/// </summary>
public interface IPluginService
{
	IEnumerable<FeedProvider> GetAvailableFeedProviders();
}
