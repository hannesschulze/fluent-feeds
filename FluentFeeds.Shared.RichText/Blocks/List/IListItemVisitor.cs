namespace FluentFeeds.Shared.RichText.Blocks.List;

/// <summary>
/// Visitor for <see cref="ListItem"/> objects.
/// </summary>
public interface IListItemVisitor
{
	void Visit(NestedListItem listItem);
	void Visit(LeafListItem listItem);
}
