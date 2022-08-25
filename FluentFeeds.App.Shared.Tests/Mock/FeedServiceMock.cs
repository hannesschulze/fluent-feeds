using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using FluentFeeds.App.Shared.Models.Feeds;
using FluentFeeds.App.Shared.Models.Feeds.Loaders;
using FluentFeeds.App.Shared.Services;
using FluentFeeds.Common;
using FluentFeeds.Feeds.Base.Feeds.Content;

namespace FluentFeeds.App.Shared.Tests.Mock;

public sealed class FeedServiceMock : IFeedService
{
	public FeedServiceMock()
	{
		_readOnlyProviderFeeds = new ReadOnlyObservableCollection<IFeedView>(ProviderFeeds);
	}
	
	public void CompleteInitialization() =>
		_initializationCompletionSource?.TrySetResult();
	
	public void CompleteInitialization(Exception exception) =>
		_initializationCompletionSource?.TrySetException(exception);
	
	public Task InitializeAsync()
	{
		var completionSource = _initializationCompletionSource = new TaskCompletionSource();
		return completionSource.Task;
	}

	public ObservableCollection<IFeedView> ProviderFeeds { get; } = new();

	ReadOnlyObservableCollection<IFeedView> IFeedService.ProviderFeeds => _readOnlyProviderFeeds;

	public IFeedView OverviewFeed { get; } =
		new Feed(
			identifier: Guid.Empty,
			storage: null,
			loaderFactory: _ => new EmptyFeedLoader(),
			hasChildren: false,
			parent: null,
			name: "Overview",
			symbol: Symbol.Home,
			metadata: new FeedMetadata(),
			isUserCustomizable: false,
			isExcludedFromGroup: false);

	private readonly ReadOnlyObservableCollection<IFeedView> _readOnlyProviderFeeds;
	private TaskCompletionSource? _initializationCompletionSource;
}
