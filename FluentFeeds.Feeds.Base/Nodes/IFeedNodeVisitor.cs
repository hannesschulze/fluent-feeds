namespace FluentFeeds.Feeds.Base.Nodes;

/// <summary>
/// Visitor for <see cref="FeedNode"/> objects.
/// </summary>
public interface IFeedNodeVisitor
{
	void Visit(IReadOnlyFeedLeafNode node);
	void Visit(IReadOnlyFeedGroupNode node);
}
