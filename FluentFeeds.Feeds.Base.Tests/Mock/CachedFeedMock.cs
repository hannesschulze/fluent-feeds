using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentFeeds.Feeds.Base.Items;
using FluentFeeds.Feeds.Base.Storage;

namespace FluentFeeds.Feeds.Base.Tests.Mock;

public sealed class CachedFeedMock : CachedFeed
{
	public CachedFeedMock(IItemStorage storage) : base(storage)
	{
	}
	
	public void CompleteFetch(IEnumerable<IReadOnlyItem> items) => _fetchCompletionSource.TrySetResult(items);
	public void CompleteFetch(params IReadOnlyItem[] items) => CompleteFetch(items.AsEnumerable());

	protected override async Task<FetchResult> DoFetchAsync()
	{
		var items = await _fetchCompletionSource.Task;
		_fetchCompletionSource = new TaskCompletionSource<IEnumerable<IReadOnlyItem>>();
		return new FetchResult(items);
	}

	private TaskCompletionSource<IEnumerable<IReadOnlyItem>> _fetchCompletionSource = new();
}
