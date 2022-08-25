using System;
using System.Threading.Tasks;
using FluentFeeds.Feeds.Base.Feeds.Content;

namespace FluentFeeds.App.Shared.Tests.Mock;

public sealed class FeedContentLoaderMock : IFeedContentLoader
{
	public void CompleteLoad(FeedContent content) => _loadCompletionSource.TrySetResult(content);
	public void CompleteLoad(Exception exception) => _loadCompletionSource.TrySetException(exception);
	
	public FeedContentLoaderMock(string identifier)
	{
		Identifier = identifier;
	}
	
	public string Identifier { get; }

	public async Task<FeedContent> LoadAsync()
	{
		var result = await _loadCompletionSource.Task;
		_loadCompletionSource = new TaskCompletionSource<FeedContent>();
		return result;
	}
	
	private TaskCompletionSource<FeedContent> _loadCompletionSource = new();
}
