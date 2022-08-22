using System;
using System.Threading.Tasks;
using FluentFeeds.Feeds.Base;

namespace FluentFeeds.App.Shared.Tests.Mock;

public sealed class FeedMock : Feed
{
	public FeedMock(Guid identifier, Uri? url = null)
	{
		Identifier = identifier;
		Url = url;
	}
		
	public Guid Identifier { get; }
	
	public Uri? Url { get; }

	public void UpdateMetadata(FeedMetadata metadata) => Metadata = metadata;
		
	protected override Task DoLoadAsync() => Task.CompletedTask;

	protected override Task DoSynchronizeAsync() => Task.CompletedTask;
}
