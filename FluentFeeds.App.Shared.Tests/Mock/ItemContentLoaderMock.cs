using System;
using System.Threading;
using System.Threading.Tasks;
using FluentFeeds.Feeds.Base.Items.Content;

namespace FluentFeeds.App.Shared.Tests.Mock;

public sealed class ItemContentLoaderMock : IItemContentLoader
{
	public void CompleteLoad(ItemContent content) => _loadCompletionSource.TrySetResult(content);
	public void CompleteLoad(Exception exception) => _loadCompletionSource.TrySetException(exception);
	
	public async Task<ItemContent> LoadAsync(bool reload = false, CancellationToken cancellation = default)
	{
		var content = await _loadCompletionSource.Task;
		_loadCompletionSource = new TaskCompletionSource<ItemContent>();
		return content;
	}

	private TaskCompletionSource<ItemContent> _loadCompletionSource = new();
}
