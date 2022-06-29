using System;
using System.Collections.Immutable;
using System.Linq;
using FluentFeeds.Shared.RichText.Helpers;

namespace FluentFeeds.Shared.RichText.Inlines;

/// <summary>
/// Base class for inlines containing multiple child inline elements.
/// </summary>
public abstract class SpanInline : Inline
{
	/// <summary>
	/// Create a new default-constructed span inline.
	/// </summary>
	protected SpanInline()
	{
		Inlines = ImmutableArray<Inline>.Empty;
	}

	/// <summary>
	/// Create a new span inline with the provided child inline elements.
	/// </summary>
	protected SpanInline(params Inline[] inlines)
	{
		Inlines = ImmutableArray.Create(inlines);
	}
	
	/// <summary>
	/// The child inline elements.
	/// </summary>
	public ImmutableArray<Inline> Inlines { get; init; }

	public override bool Equals(Inline? other)
	{
		if (ReferenceEquals(this, other))
			return true;
		if (!base.Equals(other))
			return false;
		return other is BoldInline casted && Inlines.SequenceEqual(casted.Inlines);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(base.GetHashCode(), Inlines.SequenceHashCode());
	}
}