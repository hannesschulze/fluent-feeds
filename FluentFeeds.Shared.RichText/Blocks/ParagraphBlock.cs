using System;
using System.Collections.Immutable;
using System.Linq;
using FluentFeeds.Shared.RichText.Inlines;

namespace FluentFeeds.Shared.RichText.Blocks;

/// <summary>
/// A paragraph block hosting a list of inlines.
/// </summary>
public sealed class ParagraphBlock : Block
{
	/// <summary>
	/// Create a new default-constructed paragraph block.
	/// </summary>
	public ParagraphBlock()
	{
		Inlines = ImmutableArray<Inline>.Empty;
	}

	/// <summary>
	/// Create a new paragraph block hosting the provided inlines.
	/// </summary>
	public ParagraphBlock(params Inline[] inlines)
	{
		Inlines = ImmutableArray.Create(inlines);
	}

	/// <summary>
	/// The inlines hosted in this block.
	/// </summary>
	public ImmutableArray<Inline> Inlines { get; init; }

	public override BlockType Type => BlockType.Paragraph;

	public override void Accept(IBlockVisitor visitor) => visitor.Visit(this);

	public override bool Equals(Block? other)
	{
		if (ReferenceEquals(this, other))
			return true;
		if (!base.Equals(other))
			return false;
		return other is ParagraphBlock casted && Inlines.SequenceEqual(casted.Inlines);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(
			base.GetHashCode(),
			Inlines
				.Aggregate(new HashCode(), (hash, inline) =>
				{
					hash.Add(inline);
					return hash;
				})
				.ToHashCode());
	}
}
