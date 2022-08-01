using System;
using System.Linq;
using System.Text.Json.Serialization;
using FluentFeeds.Shared.RichText.Helpers;
using FluentFeeds.Shared.RichText.Json;

namespace FluentFeeds.Shared.RichText.Inlines;

/// <summary>
/// An inline for linking to a URI.
/// </summary>
[JsonConverter(typeof(InlineJsonConverter<HyperlinkInline>))]
public sealed class HyperlinkInline : SpanInline
{
	/// <summary>
	/// Create a new default-constructed hyperlink inline.
	/// </summary>
	public HyperlinkInline()
	{
	}

	/// <summary>
	/// Create a new hyperlink inline with the provided child inline elements.
	/// </summary>
	public HyperlinkInline(params Inline[] inlines) : base(inlines)
	{
	}
	
	/// <summary>
	/// The target URI of this hyperlink.
	/// </summary>
	public Uri? Target { get; init; }

	public override InlineType Type => InlineType.Hyperlink;

	public override void Accept(IInlineVisitor visitor) => visitor.Visit(this);
	
	public override string ToString() =>
		$"HyperlinkInline {{ Inlines = {Inlines.SequenceString()}, Target = {Target?.ToString() ?? "[null]"} }}";
	
	public override bool Equals(Inline? other)
	{
		if (ReferenceEquals(this, other))
			return true;
		if (!base.Equals(other))
			return false;
		return other is HyperlinkInline casted && Target == casted.Target;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(base.GetHashCode(), Target);
	}
}
