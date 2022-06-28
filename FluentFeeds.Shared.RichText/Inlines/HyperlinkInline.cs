using System;
using System.Collections.Immutable;
using System.Linq;
using FluentFeeds.Shared.RichText.Helpers;

namespace FluentFeeds.Shared.RichText.Inlines;

/// <summary>
/// An inline for linking to a URI.
/// </summary>
public sealed class HyperlinkInline : Inline
{
	/// <summary>
	/// Create a new default-constructed hyperlink inline.
	/// </summary>
	public HyperlinkInline()
	{
		Inlines = ImmutableArray<Inline>.Empty;
	}

	/// <summary>
	/// Create a new hyperlink inline with the provided child inline elements.
	/// </summary>
	public HyperlinkInline(params Inline[] inlines)
	{
		Inlines = ImmutableArray.Create(inlines);
	}
	
	/// <summary>
	/// The child inline elements.
	/// </summary>
	public ImmutableArray<Inline> Inlines { get; init; }
	
	/// <summary>
	/// The target URI of this hyperlink.
	/// </summary>
	public Uri? Target { get; init; }

	public override InlineType Type => InlineType.Hyperlink;

	public override void Accept(IInlineVisitor visitor) => visitor.Visit(this);
	
	public override bool Equals(Inline? other)
	{
		if (ReferenceEquals(this, other))
			return true;
		if (!base.Equals(other))
			return false;
		return other is HyperlinkInline casted && Inlines.SequenceEqual(casted.Inlines) && Target == casted.Target;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(base.GetHashCode(), Inlines.SequenceHashCode(), Target);
	}
}
