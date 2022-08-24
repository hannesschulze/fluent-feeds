using System.Text.Json.Serialization;
using FluentFeeds.Common.Helpers;
using FluentFeeds.Documents.Json;

namespace FluentFeeds.Documents.Inlines;

/// <summary>
/// An inline for making text strikethrough.
/// </summary>
[JsonConverter(typeof(InlineJsonConverter<StrikethroughInline>))]
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
	
	public override string ToString() => $"StrikethroughInline {{ Inlines = {Inlines.SequenceString()} }}";
	
	public override bool Equals(Inline? other)
	{
		if (ReferenceEquals(this, other))
			return true;
		return base.Equals(other) && other is StrikethroughInline;
	}
}
