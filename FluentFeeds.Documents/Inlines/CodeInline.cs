using System.Text.Json.Serialization;
using FluentFeeds.Common.Helpers;
using FluentFeeds.Documents.Json;

namespace FluentFeeds.Documents.Inlines;

/// <summary>
/// An inline for formatting text as code.
/// </summary>
[JsonConverter(typeof(InlineJsonConverter<CodeInline>))]
public sealed class CodeInline : SpanInline
{
	/// <summary>
	/// Create a new default-constructed code inline.
	/// </summary>
	public CodeInline()
	{
	}

	/// <summary>
	/// Create a new code inline with the provided child inline elements.
	/// </summary>
	public CodeInline(params Inline[] inlines) : base(inlines)
	{
	}

	public override InlineType Type => InlineType.Code;

	public override void Accept(IInlineVisitor visitor) => visitor.Visit(this);
	
	public override string ToString() => $"CodeInline {{ Inlines = {Inlines.SequenceString()} }}";
	
	public override bool Equals(Inline? other)
	{
		if (ReferenceEquals(this, other))
			return true;
		return base.Equals(other) && other is CodeInline;
	}
}
