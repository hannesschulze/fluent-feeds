using System;
using System.Collections.Immutable;
using System.Linq;
using System.Text.Json.Serialization;
using FluentFeeds.Shared.RichText.Helpers;
using FluentFeeds.Shared.RichText.Inlines;
using FluentFeeds.Shared.RichText.Json;

namespace FluentFeeds.Shared.RichText.Blocks.Table;

/// <summary>
/// A cell in a table row which can host multiple blocks.
/// <seealso cref="TableBlock"/>
/// </summary>
[JsonConverter(typeof(TableCellJsonConverter))]
public sealed class TableCell
{
	/// <summary>
	/// Create a new default-constructed table cell.
	/// </summary>
	public TableCell()
	{
		Blocks = ImmutableArray<Block>.Empty;
	}

	/// <summary>
	/// Create a new table cell from a list of blocks.
	/// </summary>
	public TableCell(params Block[] blocks)
	{
		Blocks = ImmutableArray.Create(blocks);
	}

	/// <summary>
	/// Create a new table cell hosting a single generic block with the provided inline elements.
	/// </summary>
	public TableCell(params Inline[] inlines)
	{
		Blocks = ImmutableArray.Create<Block>(new GenericBlock(inlines));
	}
	
	/// <summary>
	/// The cell's content.
	/// </summary>
	public ImmutableArray<Block> Blocks { get; init; }

	/// <summary>
	/// Defines over how many columns the cell is extended.
	/// </summary>
	public int ColumnSpan { get; init; } = 1;
	
	/// <summary>
	/// Defines over how many rows the cell is extended.
	/// </summary>
	public int RowSpan { get; init; } = 1;

	/// <summary>
	/// Flag indicating if this cell should be rendered with a table-header-like appearance.
	/// </summary>
	public bool IsHeader { get; init; } = false;

	public bool Equals(TableCell? other)
	{
		if (ReferenceEquals(this, other))
			return true;
		if (ReferenceEquals(null, other))
			return false;
		return Blocks.SequenceEqual(other.Blocks) && ColumnSpan == other.ColumnSpan && RowSpan == other.RowSpan &&
		       IsHeader == other.IsHeader;
	}

	public override bool Equals(object? obj)
	{
		if (ReferenceEquals(this, obj))
			return true;
		return obj is TableCell other && Equals(other);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(Blocks.SequenceHashCode(), ColumnSpan, RowSpan, IsHeader);
	}
	
	public override string ToString() =>
		$"TableCell {{ Blocks = {Blocks.SequenceString()}, ColumnSpan = {ColumnSpan}, RowSpan = {RowSpan}, " +
		$"IsHeader = {IsHeader} }}";

	public static bool operator ==(TableCell? lhs, TableCell? rhs) => lhs?.Equals(rhs) ?? ReferenceEquals(rhs, null);
	public static bool operator !=(TableCell? lhs, TableCell? rhs) => !(lhs == rhs);
}
