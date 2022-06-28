using System;
using System.Collections.Immutable;
using System.Linq;
using FluentFeeds.Shared.RichText.Helpers;

namespace FluentFeeds.Shared.RichText.Inlines;

/// <summary>
/// An inline for formatting text as code.
/// </summary>
public sealed class CodeInline : Inline
{
	/// <summary>
	/// Create a new default-constructed code inline.
	/// </summary>
	public CodeInline()
	{
		Inlines = ImmutableArray<Inline>.Empty;
	}

	/// <summary>
	/// Create a new code inline with the provided child inline elements.
	/// </summary>
	public CodeInline(params Inline[] inlines)
	{
		Inlines = ImmutableArray.Create(inlines);
	}
	
	/// <summary>
	/// The child inline elements.
	/// </summary>
	public ImmutableArray<Inline> Inlines { get; init; }

	public override InlineType Type => InlineType.Code;

	public override void Accept(IInlineVisitor visitor) => visitor.Visit(this);
	
	public override bool Equals(Inline? other)
	{
		if (ReferenceEquals(this, other))
			return true;
		if (!base.Equals(other))
			return false;
		return other is CodeInline casted && Inlines.SequenceEqual(casted.Inlines);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(base.GetHashCode(), Inlines.SequenceHashCode());
	}
}
