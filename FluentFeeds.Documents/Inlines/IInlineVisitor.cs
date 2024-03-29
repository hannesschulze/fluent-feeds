namespace FluentFeeds.Documents.Inlines;

/// <summary>
/// Visitor for <see cref="Inline"/> elements.
/// </summary>
public interface IInlineVisitor
{
	void Visit(TextInline inline);
	void Visit(ImageInline inline);
	void Visit(BoldInline inline);
	void Visit(ItalicInline inline);
	void Visit(UnderlineInline inline);
	void Visit(StrikethroughInline inline);
	void Visit(CodeInline inline);
	void Visit(HyperlinkInline inline);
}
