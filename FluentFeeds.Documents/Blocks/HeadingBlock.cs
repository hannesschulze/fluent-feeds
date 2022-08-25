using System;
using System.Collections.Immutable;
using System.Linq;
using System.Text.Json.Serialization;
using FluentFeeds.Common.Helpers;
using FluentFeeds.Documents.Blocks.Heading;
using FluentFeeds.Documents.Inlines;
using FluentFeeds.Documents.Json;

namespace FluentFeeds.Documents.Blocks;

/// <summary>
/// A heading block hosting a group of inlines.
/// </summary>
[JsonConverter(typeof(BlockJsonConverter<HeadingBlock>))]
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
	
	public override string ToString() => $"HeadingBlock {{ Inlines = {Inlines.SequenceString()}, Level = {Level} }}";

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
