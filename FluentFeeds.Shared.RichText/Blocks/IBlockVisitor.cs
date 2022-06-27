namespace FluentFeeds.Shared.RichText.Blocks;

/// <summary>
/// Visitor for <see cref="Block"/> objects.
/// </summary>
public interface IBlockVisitor
{
	void Visit(ParagraphBlock block);
}
