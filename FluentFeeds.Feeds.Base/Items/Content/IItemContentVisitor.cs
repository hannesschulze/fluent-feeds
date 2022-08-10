namespace FluentFeeds.Feeds.Base.Items.Content;

/// <summary>
/// Visitor for <see cref="ItemContent"/> objects.
/// </summary>
public interface IItemContentVisitor
{
	void Visit(ArticleItemContent itemContent);
}
