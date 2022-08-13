using System.Collections.ObjectModel;
using System.Threading.Tasks;
using FluentFeeds.App.Shared.Models;
using FluentFeeds.App.Shared.Services;
using FluentFeeds.Common;
using FluentFeeds.Feeds.Base;
using FluentFeeds.Feeds.Base.Nodes;

namespace FluentFeeds.App.Shared.Tests.Services.Mock;

public sealed class FeedServiceMock : IFeedService
{
	public FeedServiceMock()
	{
		FeedProviders = new ReadOnlyObservableCollection<LoadedFeedProvider>(_feedProviders);
	}
	
	public Task LoadFeedProvidersAsync() => Task.CompletedTask;

	public ReadOnlyObservableCollection<LoadedFeedProvider> FeedProviders { get; }

	public IReadOnlyFeedNode OverviewFeed { get; } =
		FeedNode.Custom(new EmptyFeed(), "Overview", Symbol.Home, isUserCustomizable: false);

	private readonly ObservableCollection<LoadedFeedProvider> _feedProviders = new();
}
