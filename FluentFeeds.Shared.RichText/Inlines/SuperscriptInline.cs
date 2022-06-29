namespace FluentFeeds.Shared.RichText.Inlines;

/// <summary>
/// An inline for making text superscript, i.e. appearing above normal text.
/// </summary>
public sealed class SuperscriptInline : SpanInline
{
	/// <summary>
	/// Create a new default-constructed superscript inline.
	/// </summary>
	public SuperscriptInline()
	{
	}

	/// <summary>
	/// Create a new superscript inline with the provided child inline elements.
	/// </summary>
	public SuperscriptInline(params Inline[] inlines) : base(inlines)
	{
	}

	public override InlineType Type => InlineType.Superscript;

	public override void Accept(IInlineVisitor visitor) => visitor.Visit(this);
	
	public override bool Equals(Inline? other)
	{
		if (ReferenceEquals(this, other))
			return true;
		return base.Equals(other) && other is SuperscriptInline;
	}
}
