namespace FluentFeeds.Feeds.Base.Items;

/// <summary>
/// Visitor for <see cref="Item"/> objects.
/// </summary>
public interface IItemVisitor
{
	void Visit(IReadOnlyArticleItem item);
}
