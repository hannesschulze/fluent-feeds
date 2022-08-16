using System;
using System.Threading.Tasks;
using FluentFeeds.Feeds.Base;

namespace FluentFeeds.App.Shared.Tests.Services.Mock;

public sealed class FeedMock : Feed
{
	public FeedMock(Guid identifier)
	{
		Identifier = identifier;
	}
		
	public Guid Identifier { get; }

	public void UpdateMetadata(FeedMetadata metadata) => Metadata = metadata;
		
	protected override Task DoLoadAsync() => Task.CompletedTask;

	protected override Task DoSynchronizeAsync() => Task.CompletedTask;
}
