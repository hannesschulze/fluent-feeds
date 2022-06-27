namespace FluentFeeds.Shared.RichText.Inlines;

/// <summary>
/// Visitor for <see cref="Inline"/> elements.
/// </summary>
public interface IInlineVisitor
{
	void Visit(TextInline inline);
}
