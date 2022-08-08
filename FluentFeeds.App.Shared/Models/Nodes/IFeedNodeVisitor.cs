namespace FluentFeeds.App.Shared.Models.Nodes;

/// <summary>
/// Visitor for <see cref="FeedNode"/> objects.
/// </summary>
public interface IFeedNodeVisitor
{
	void Visit(FeedItem node);
	void Visit(FeedGroup node);
}
