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
		_readOnlyFeedProviders = new ReadOnlyObservableCollection<LoadedFeedProvider>(FeedProviders);
	}
	
	public Task InitializeAsync() => Task.CompletedTask;

	public ObservableCollection<LoadedFeedProvider> FeedProviders { get; } = new();

	ReadOnlyObservableCollection<LoadedFeedProvider> IFeedService.FeedProviders => _readOnlyFeedProviders;

	public IReadOnlyFeedNode OverviewFeed { get; } =
		FeedNode.Custom(new EmptyFeed(), "Overview", Symbol.Home, isUserCustomizable: false);

	private readonly ReadOnlyObservableCollection<LoadedFeedProvider> _readOnlyFeedProviders;
}
