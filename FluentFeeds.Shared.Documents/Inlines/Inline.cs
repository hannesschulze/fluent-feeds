using System;
using System.Text.Json.Serialization;
using FluentFeeds.Shared.Documents.Blocks;
using FluentFeeds.Shared.Documents.Html;
using FluentFeeds.Shared.Documents.Json;
using FluentFeeds.Shared.Documents.PlainText;

namespace FluentFeeds.Shared.Documents.Inlines;

/// <summary>
/// <para>Base class for inline rich text elements.</para>
///
/// <para>Inlines can be nested in other inlines or as a part of a block (see <see cref="ParagraphBlock"/>). There are
/// multiple container inlines which can change the format of other inlines, as well as leaf inlines like text or image
/// inlines.</para>
/// </summary>
[JsonConverter(typeof(InlineJsonConverter<Inline>))]
public abstract class Inline : IEquatable<Inline>
{
	/// <summary>
	/// The type of this inline element.
	/// </summary>
	public abstract InlineType Type { get; }

	/// <summary>
	/// Accept a visitor for this inline element.
	/// </summary>
	public abstract void Accept(IInlineVisitor visitor);
	
	/// <summary>
	/// Format this rich text inline element as a HTML string.
	/// </summary>
	public string ToHtml(HtmlWritingOptions? options = null)
	{
		var writer = new HtmlWriter(options ?? new HtmlWritingOptions());
		Accept(writer);
		return writer.GetResult();
	}

	/// <summary>
	/// Extract the plain text from this rich text inline element.
	/// </summary>
	public string ToPlainText()
	{
		var writer = new PlainTextWriter();
		Accept(writer);
		return writer.GetResult();
	}
	
	public virtual bool Equals(Inline? other)
	{
		if (ReferenceEquals(this, other))
			return true;
		if (ReferenceEquals(null, other))
			return false;
		return true;
	}

	public override bool Equals(object? obj)
	{
		if (ReferenceEquals(this, obj))
			return true;
		return obj is Inline other && Equals(other);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(Type);
	}

	public override string ToString() => $"Inline {{ Type = {Type} }}";

	public static bool operator ==(Inline? lhs, Inline? rhs) => lhs?.Equals(rhs) ?? ReferenceEquals(rhs, null);
	public static bool operator !=(Inline? lhs, Inline? rhs) => !(lhs == rhs);
}
