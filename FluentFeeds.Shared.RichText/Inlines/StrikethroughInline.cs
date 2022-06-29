namespace FluentFeeds.Shared.RichText.Inlines;

/// <summary>
/// An inline for making text strikethrough.
/// </summary>
public sealed class StrikethroughInline : SpanInline
{
	/// <summary>
	/// Create a new default-constructed strikethrough inline.
	/// </summary>
	public StrikethroughInline()
	{
	}

	/// <summary>
	/// Create a new strikethrough inline with the provided child inline elements.
	/// </summary>
	public StrikethroughInline(params Inline[] inlines) : base(inlines)
	{
	}

	public override InlineType Type => InlineType.Strikethrough;

	public override void Accept(IInlineVisitor visitor) => visitor.Visit(this);
	
	public override bool Equals(Inline? other)
	{
		if (ReferenceEquals(this, other))
			return true;
		return base.Equals(other) && other is StrikethroughInline;
	}
}
