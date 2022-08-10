using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using FluentFeeds.Feeds.Base.Items;

namespace FluentFeeds.Feeds.Base.Tests.Mock;

public sealed class FeedMock : Feed
{
	
	public void CompleteLoad(IEnumerable<IReadOnlyStoredItem> items) => _loadCompletionSource?.TrySetResult(items);
	public void CompleteLoad(params IReadOnlyStoredItem[] items) => CompleteLoad(items.AsEnumerable());
	public void CompleteLoad(Exception exception) => _loadCompletionSource?.TrySetException(exception);

	public void CompleteSynchronize(IEnumerable<IReadOnlyStoredItem> items) =>
		_synchronizeCompletionSource?.TrySetResult(items);
	public void CompleteSynchronize(params IReadOnlyStoredItem[] items) =>
		CompleteSynchronize(items.AsEnumerable());
	public void CompleteSynchronize(Exception exception) => _synchronizeCompletionSource?.TrySetException(exception);

	public void UpdateItems(params IReadOnlyStoredItem[] items) => Items = items.ToImmutableHashSet();

	public void UpdateMetadata(FeedMetadata? metadata) => Metadata = metadata;

	protected override Task<IEnumerable<IReadOnlyStoredItem>> DoLoadAsync()
	{
		var completionSource = _loadCompletionSource = new TaskCompletionSource<IEnumerable<IReadOnlyStoredItem>>();
		return completionSource.Task;
	}

	protected override Task<IEnumerable<IReadOnlyStoredItem>> DoSynchronizeAsync()
	{
		var completionSource = _synchronizeCompletionSource =
			new TaskCompletionSource<IEnumerable<IReadOnlyStoredItem>>();
		return completionSource.Task;
	}

	private TaskCompletionSource<IEnumerable<IReadOnlyStoredItem>>? _loadCompletionSource;
	private TaskCompletionSource<IEnumerable<IReadOnlyStoredItem>>? _synchronizeCompletionSource;
}
