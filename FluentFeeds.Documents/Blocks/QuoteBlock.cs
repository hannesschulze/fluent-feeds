using System;
using System.Collections.Immutable;
using System.Linq;
using System.Text.Json.Serialization;
using FluentFeeds.Documents.Json;
using FluentFeeds.Documents.Helpers;

namespace FluentFeeds.Documents.Blocks;

/// <summary>
/// A blockquote which can contain multiple child blocks.
/// </summary>
[JsonConverter(typeof(BlockJsonConverter<QuoteBlock>))]
public sealed class QuoteBlock : Block
{
	/// <summary>
	/// Create a new default-constructed quote block.
	/// </summary>
	public QuoteBlock()
	{
		Blocks = ImmutableArray<Block>.Empty;
	}

	/// <summary>
	/// Create a new quote block hosting the provided blocks.
	/// </summary>
	public QuoteBlock(params Block[] blocks)
	{
		Blocks = ImmutableArray.Create(blocks);
	}

	/// <summary>
	/// The blocks displayed as a quote.
	/// </summary>
	public ImmutableArray<Block> Blocks { get; init; }

	public override BlockType Type => BlockType.Quote;

	public override void Accept(IBlockVisitor visitor) => visitor.Visit(this);
	
	public override string ToString() => $"QuoteBlock {{ Blocks = {Blocks.SequenceString()} }}";

	public override bool Equals(Block? other)
	{
		if (ReferenceEquals(this, other))
			return true;
		if (!base.Equals(other))
			return false;
		return other is QuoteBlock casted && Blocks.SequenceEqual(casted.Blocks);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(base.GetHashCode(), Blocks.SequenceHashCode());
	}
}
