namespace FluentFeeds.Shared.RichText.Blocks;

/// <summary>
/// Visitor for <see cref="Block"/> objects.
/// </summary>
public interface IBlockVisitor
{
	void Visit(ParagraphBlock block);
	void Visit(CodeBlock block);
	void Visit(HeadingBlock block);
	void Visit(HorizontalRuleBlock block);
	void Visit(ListBlock block);
	void Visit(QuoteBlock block);
}
