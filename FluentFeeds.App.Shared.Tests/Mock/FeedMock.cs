using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using FluentFeeds.App.Shared.Models.Feeds.Loaders;
using FluentFeeds.App.Shared.Models.Items;

namespace FluentFeeds.App.Shared.Tests.Mock;

public sealed class FeedLoaderMock : FeedLoader
{
	public void CompleteInitialize(IEnumerable<IItemView> items) => _initializeCompletionSource.TrySetResult(items);
	public void CompleteInitialize(params IItemView[] items) => CompleteInitialize(items.AsEnumerable());
	public void CompleteInitialize(Exception exception) => _initializeCompletionSource.TrySetException(exception);

	public void CompleteSynchronize(IEnumerable<IItemView> items) => _synchronizeCompletionSource.TrySetResult(items);
	public void CompleteSynchronize(params IItemView[] items) => CompleteSynchronize(items.AsEnumerable());
	public void CompleteSynchronize(Exception exception) => _synchronizeCompletionSource.TrySetException(exception);

	public void UpdateItems(params IItemView[] items) => Items = items.ToImmutableHashSet();

	public void UpdateIsLoadingCustom(bool isLoadingCustom) => IsLoadingCustom = isLoadingCustom;

	protected override async Task DoInitializeAsync()
	{
		var items = await _initializeCompletionSource.Task;
		_initializeCompletionSource = new TaskCompletionSource<IEnumerable<IItemView>>();
		Items = items.ToImmutableHashSet();
	}

	protected override async Task DoSynchronizeAsync()
	{
		var items = await _synchronizeCompletionSource.Task;
		_synchronizeCompletionSource = new TaskCompletionSource<IEnumerable<IItemView>>();
		Items = items.ToImmutableHashSet();
	}

	private TaskCompletionSource<IEnumerable<IItemView>> _initializeCompletionSource = new();
	private TaskCompletionSource<IEnumerable<IItemView>> _synchronizeCompletionSource = new();
}
