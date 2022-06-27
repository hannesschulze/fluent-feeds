using System;
using FluentFeeds.Shared.RichText.Blocks;

namespace FluentFeeds.Shared.RichText.Inlines;

/// <summary>
/// <para>Base class for inline rich text elements.</para>
///
/// <para>Inlines can be nested in other inlines or as a part of a block (see <see cref="ParagraphBlock"/>). There are
/// multiple container inlines which can change the format of other inlines, as well as leaf inlines like text or image
/// inlines.</para>
/// </summary>
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

	public static bool operator ==(Inline lhs, Inline rhs) => lhs.Equals(rhs);
	public static bool operator !=(Inline lhs, Inline rhs) => !lhs.Equals(rhs);
}
