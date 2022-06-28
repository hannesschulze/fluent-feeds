namespace FluentFeeds.Shared.RichText.Inlines;

/// <summary>
/// An inline for making text underlined.
/// </summary>
public sealed class UnderlineInline : SpanInline
{
	/// <summary>
	/// Create a new default-constructed underline inline.
	/// </summary>
	public UnderlineInline()
	{
	}

	/// <summary>
	/// Create a new underline inline with the provided child inline elements.
	/// </summary>
	public UnderlineInline(params Inline[] inlines) : base(inlines)
	{
	}

	public override InlineType Type => InlineType.Underline;

	public override void Accept(IInlineVisitor visitor) => visitor.Visit(this);
	
	public override bool Equals(Inline? other)
	{
		if (ReferenceEquals(this, other))
			return true;
		return base.Equals(other) && other is UnderlineInline;
	}
}
