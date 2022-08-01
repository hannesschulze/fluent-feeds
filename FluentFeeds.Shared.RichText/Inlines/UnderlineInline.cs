using System.Text.Json.Serialization;
using FluentFeeds.Shared.RichText.Helpers;
using FluentFeeds.Shared.RichText.Json;

namespace FluentFeeds.Shared.RichText.Inlines;

/// <summary>
/// An inline for making text underlined.
/// </summary>
[JsonConverter(typeof(InlineJsonConverter<UnderlineInline>))]
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
	public override string ToString() => $"UnderlineInline {{ Inlines = {Inlines.SequenceString()} }}";
	
	public override bool Equals(Inline? other)
	{
		if (ReferenceEquals(this, other))
			return true;
		return base.Equals(other) && other is UnderlineInline;
	}
}
