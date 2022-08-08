namespace FluentFeeds.Shared.Models.Nodes;

public abstract class FeedItem : FeedNode
{
	public Feed Feed { get; }
	
	protected abstract Feed DoCreateFeed();
}
