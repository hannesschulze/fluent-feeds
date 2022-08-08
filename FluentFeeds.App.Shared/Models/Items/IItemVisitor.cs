namespace FluentFeeds.App.Shared.Models.Items;

/// <summary>
/// Visitor for <see cref="Item"/> objects.
/// </summary>
public interface IItemVisitor
{
	void Visit(ArticleItem item);
}
