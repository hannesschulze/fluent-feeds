using System;
using System.Collections.Immutable;
using System.Linq;
using FluentFeeds.Shared.RichText.Helpers;

namespace FluentFeeds.Shared.RichText.Inlines;

/// <summary>
/// An inline for making text italic.
/// </summary>
public sealed class ItalicInline : Inline
{
	/// <summary>
	/// Create a new default-constructed italic inline.
	/// </summary>
	public ItalicInline()
	{
		Inlines = ImmutableArray<Inline>.Empty;
	}

	/// <summary>
	/// Create a new italic inline with the provided child inline elements.
	/// </summary>
	public ItalicInline(params Inline[] inlines)
	{
		Inlines = ImmutableArray.Create(inlines);
	}
	
	/// <summary>
	/// The child inline elements.
	/// </summary>
	public ImmutableArray<Inline> Inlines { get; init; }

	public override InlineType Type => InlineType.Italic;

	public override void Accept(IInlineVisitor visitor) => visitor.Visit(this);
	
	public override bool Equals(Inline? other)
	{
		if (ReferenceEquals(this, other))
			return true;
		if (!base.Equals(other))
			return false;
		return other is ItalicInline casted && Inlines.SequenceEqual(casted.Inlines);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(base.GetHashCode(), Inlines.SequenceHashCode());
	}
}
