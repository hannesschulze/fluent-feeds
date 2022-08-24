using System;
using System.Collections.Immutable;
using System.Linq;
using System.Text.Json.Serialization;
using FluentFeeds.Common.Helpers;
using FluentFeeds.Documents.Inlines;
using FluentFeeds.Documents.Json;

namespace FluentFeeds.Documents.Blocks;

/// <summary>
/// <para>A generic block hosting inline elements.</para>
///
/// <para>Unlike <see cref="ParagraphBlock"/>, this block type does not have a semantic meaning.</para>
/// </summary>
[JsonConverter(typeof(BlockJsonConverter<GenericBlock>))]
public sealed class GenericBlock : Block
{
	/// <summary>
	/// Create a new default-constructed generic block.
	/// </summary>
	public GenericBlock()
	{
		Inlines = ImmutableArray<Inline>.Empty;
	}

	/// <summary>
	/// Create a new generic block hosting the provided inlines.
	/// </summary>
	public GenericBlock(params Inline[] inlines)
	{
		Inlines = ImmutableArray.Create(inlines);
	}

	/// <summary>
	/// The inlines hosted in this block.
	/// </summary>
	public ImmutableArray<Inline> Inlines { get; init; }

	public override BlockType Type => BlockType.Generic;

	public override void Accept(IBlockVisitor visitor) => visitor.Visit(this);
	
	public override string ToString() => $"GenericBlock {{ Inlines = {Inlines.SequenceString()} }}";

	public override bool Equals(Block? other)
	{
		if (ReferenceEquals(this, other))
			return true;
		if (!base.Equals(other))
			return false;
		return other is GenericBlock casted && Inlines.SequenceEqual(casted.Inlines);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(base.GetHashCode(), Inlines.SequenceHashCode());
	}
}
