using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using FluentFeeds.App.Shared.Services;
using FluentFeeds.Common;
using FluentFeeds.Feeds.Base;
using FluentFeeds.Feeds.Base.Nodes;

namespace FluentFeeds.App.Shared.Tests.Mock;

public sealed class FeedServiceMock : IFeedService
{
	public FeedServiceMock()
	{
		_readOnlyProviderNodes = new ReadOnlyObservableCollection<IReadOnlyStoredFeedNode>(ProviderNodes);
	}
	
	public void CompleteInitialization() =>
		_initializationCompletionSource.TrySetResult();
	
	public void CompleteInitialization(Exception exception) =>
		_initializationCompletionSource.TrySetException(exception);
	
	public async Task InitializeAsync()
	{
		await _initializationCompletionSource.Task;
		_initializationCompletionSource = new TaskCompletionSource();
	}

	public ObservableCollection<IReadOnlyStoredFeedNode> ProviderNodes { get; } = new();

	ReadOnlyObservableCollection<IReadOnlyStoredFeedNode> IFeedService.ProviderNodes => _readOnlyProviderNodes;

	public IReadOnlyFeedNode OverviewNode { get; } =
		FeedNode.Custom(new EmptyFeed(), "Overview", Symbol.Home, isUserCustomizable: false);

	private readonly ReadOnlyObservableCollection<IReadOnlyStoredFeedNode> _readOnlyProviderNodes;
	private TaskCompletionSource _initializationCompletionSource = new();
}
