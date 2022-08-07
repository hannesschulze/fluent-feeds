using System;
using System.Collections.Immutable;
using System.Linq;
using System.Text.Json.Serialization;
using FluentFeeds.Shared.Documents.Inlines;
using FluentFeeds.Shared.Documents.Json;
using FluentFeeds.Shared.Documents.Helpers;

namespace FluentFeeds.Shared.Documents.Blocks;

/// <summary>
/// A paragraph block hosting a group of inlines.
/// </summary>
[JsonConverter(typeof(BlockJsonConverter<ParagraphBlock>))]
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
	
	public override string ToString() => $"ParagraphBlock {{ Inlines = {Inlines.SequenceString()} }}";

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
		return HashCode.Combine(base.GetHashCode(), Inlines.SequenceHashCode());
	}
}
