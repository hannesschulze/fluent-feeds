using System;
using System.Text.Json.Serialization;
using FluentFeeds.Shared.Documents.Json;

namespace FluentFeeds.Shared.Documents.Inlines;

/// <summary>
/// Inline for defining plain-text runs.
/// </summary>
[JsonConverter(typeof(InlineJsonConverter<TextInline>))]
public sealed class TextInline : Inline
{
	/// <summary>
	/// Create a new default-constructed text inline.
	/// </summary>
	public TextInline()
	{
		Text = String.Empty;
	}

	/// <summary>
	/// Create a new text inline with the provided text content.
	/// </summary>
	public TextInline(string text)
	{
		Text = text;
	}
	
	/// <summary>
	/// The text for this run.
	/// </summary>
	public string Text { get; init; }

	public override InlineType Type => InlineType.Text;

	public override void Accept(IInlineVisitor visitor) => visitor.Visit(this);
	
	public override string ToString() => $"TextInline {{ Text = {Text} }}";
	
	public override bool Equals(Inline? other)
	{
		if (ReferenceEquals(this, other))
			return true;
		if (!base.Equals(other))
			return false;
		return other is TextInline casted && Text == casted.Text;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(base.GetHashCode(), Text);
	}
}
