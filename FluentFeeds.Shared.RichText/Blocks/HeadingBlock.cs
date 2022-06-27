using System;
using System.Collections.Immutable;
using System.Linq;
using FluentFeeds.Shared.RichText.Blocks.Heading;
using FluentFeeds.Shared.RichText.Helpers;
using FluentFeeds.Shared.RichText.Inlines;

namespace FluentFeeds.Shared.RichText.Blocks;

/// <summary>
/// A heading block hosting a group of inlines.
/// </summary>
public sealed class HeadingBlock : Block
{
	/// <summary>
	/// Create a new default-constructed heading block.
	/// </summary>
	public HeadingBlock()
	{
		Inlines = ImmutableArray<Inline>.Empty;
	}

	/// <summary>
	/// Create a new heading block hosting the provided inlines.
	/// </summary>
	public HeadingBlock(params Inline[] inlines)
	{
		Inlines = ImmutableArray.Create(inlines);
	}

	/// <summary>
	/// The inlines hosted in this block.
	/// </summary>
	public ImmutableArray<Inline> Inlines { get; init; }

	/// <summary>
	/// The level of this heading.
	/// </summary>
	public HeadingLevel Level { get; init; } = HeadingLevel.Level1;

	public override BlockType Type => BlockType.Heading;

	public override void Accept(IBlockVisitor visitor) => visitor.Visit(this);

	public override bool Equals(Block? other)
	{
		if (ReferenceEquals(this, other))
			return true;
		if (!base.Equals(other))
			return false;
		return other is HeadingBlock casted &&
		       Inlines.SequenceEqual(casted.Inlines) && 
		       Level == casted.Level;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(base.GetHashCode(), Inlines.SequenceHashCode(), Level);
	}
}
