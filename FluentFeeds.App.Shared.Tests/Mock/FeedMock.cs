using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using FluentFeeds.Feeds.Base;
using FluentFeeds.Feeds.Base.Items;

namespace FluentFeeds.App.Shared.Tests.Mock;

public sealed class FeedMock : Feed
{
	public void CompleteSynchronize(IEnumerable<IReadOnlyStoredItem> items) =>
		_synchronizeCompletionSource.TrySetResult(items);
	public void CompleteSynchronize(params IReadOnlyStoredItem[] items) =>
		CompleteSynchronize(items.AsEnumerable());
	public void CompleteSynchronize(Exception exception) => _synchronizeCompletionSource.TrySetException(exception);

	public void UpdateItems(params IReadOnlyStoredItem[] items) => Items = items.ToImmutableHashSet();
	
	public FeedMock(Guid identifier, Uri? url = null)
	{
		Identifier = identifier;
		Url = url;
	}
		
	public Guid Identifier { get; }
	
	public Uri? Url { get; }

	public void UpdateMetadata(FeedMetadata metadata) => Metadata = metadata;

	protected override Task DoLoadAsync() => Task.CompletedTask;

	protected override async Task DoSynchronizeAsync()
	{
		var items = await _synchronizeCompletionSource.Task;
		_synchronizeCompletionSource = new TaskCompletionSource<IEnumerable<IReadOnlyStoredItem>>();
		Items = items.ToImmutableHashSet();
	}

	private TaskCompletionSource<IEnumerable<IReadOnlyStoredItem>> _synchronizeCompletionSource = new();
}
