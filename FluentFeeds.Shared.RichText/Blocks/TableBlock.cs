using System;
using System.Collections.Immutable;
using System.Linq;
using System.Text.Json.Serialization;
using FluentFeeds.Shared.RichText.Blocks.Table;
using FluentFeeds.Shared.RichText.Helpers;
using FluentFeeds.Shared.RichText.Json;

namespace FluentFeeds.Shared.RichText.Blocks;

/// <summary>
/// A block for rendering tabular data.
/// </summary>
[JsonConverter(typeof(BlockJsonConverter<TableBlock>))]
public sealed class TableBlock : Block
{
	/// <summary>
	/// Create a new default-constructed table block.
	/// </summary>
	public TableBlock()
	{
		Rows = ImmutableArray<TableRow>.Empty;
	}

	/// <summary>
	/// Create a new table block containing the provided rows.
	/// </summary>
	public TableBlock(params TableRow[] rows)
	{
		Rows = ImmutableArray.Create(rows);
	}

	/// <summary>
	/// The rows of the table.
	/// </summary>
	public ImmutableArray<TableRow> Rows { get; init; }

	public override BlockType Type => BlockType.Table;

	public override void Accept(IBlockVisitor visitor) => visitor.Visit(this);
	public override string ToString() => $"TableBlock {{ Rows = {Rows.SequenceString()} }}";

	public override bool Equals(Block? other)
	{
		if (ReferenceEquals(this, other))
			return true;
		if (!base.Equals(other))
			return false;
		return other is TableBlock casted && Rows.SequenceEqual(casted.Rows);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(base.GetHashCode(), Rows.SequenceHashCode());
	}
}
