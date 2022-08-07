using System.Text.Json.Serialization;
using FluentFeeds.Shared.Documents.Json;
using FluentFeeds.Shared.Documents.Helpers;

namespace FluentFeeds.Shared.Documents.Inlines;

/// <summary>
/// An inline for making text italic.
/// </summary>
[JsonConverter(typeof(InlineJsonConverter<ItalicInline>))]
public sealed class ItalicInline : SpanInline
{
	/// <summary>
	/// Create a new default-constructed italic inline.
	/// </summary>
	public ItalicInline()
	{
	}

	/// <summary>
	/// Create a new italic inline with the provided child inline elements.
	/// </summary>
	public ItalicInline(params Inline[] inlines) : base(inlines)
	{
	}

	public override InlineType Type => InlineType.Italic;

	public override void Accept(IInlineVisitor visitor) => visitor.Visit(this);
	
	public override string ToString() => $"ItalicInline {{ Inlines = {Inlines.SequenceString()} }}";
	
	public override bool Equals(Inline? other)
	{
		if (ReferenceEquals(this, other))
			return true;
		return base.Equals(other) && other is ItalicInline;
	}
}
