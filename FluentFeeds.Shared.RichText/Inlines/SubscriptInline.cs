namespace FluentFeeds.Shared.RichText.Inlines;

/// <summary>
/// An inline for making text subscript, i.e. appearing below normal text.
/// </summary>
public sealed class SubscriptInline : SpanInline
{
	/// <summary>
	/// Create a new default-constructed subscript inline.
	/// </summary>
	public SubscriptInline()
	{
	}

	/// <summary>
	/// Create a new subscript inline with the provided child inline elements.
	/// </summary>
	public SubscriptInline(params Inline[] inlines) : base(inlines)
	{
	}

	public override InlineType Type => InlineType.Subscript;

	public override void Accept(IInlineVisitor visitor) => visitor.Visit(this);
	
	public override bool Equals(Inline? other)
	{
		if (ReferenceEquals(this, other))
			return true;
		return base.Equals(other) && other is SubscriptInline;
	}
}
