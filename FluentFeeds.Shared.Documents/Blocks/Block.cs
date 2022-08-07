using System;
using System.Text.Json.Serialization;
using FluentFeeds.Shared.Documents.Html;
using FluentFeeds.Shared.Documents.Json;

namespace FluentFeeds.Shared.Documents.Blocks;

/// <summary>
/// <para>Base class of rich text blocks.</para>
///
/// <para>Rich text objects are made up of multiple blocks. There are multiple types of blocks which can be used to
/// define a document – for example, paragraph blocks containing text (in the form of inlines) or table blocks.</para>
/// </summary>
[JsonConverter(typeof(BlockJsonConverter<Block>))]
public abstract class Block : IEquatable<Block>
{
	/// <summary>
	/// The type of this block.
	/// </summary>
	public abstract BlockType Type { get; }

	/// <summary>
	/// Accept a visitor for this block object.
	/// </summary>
	public abstract void Accept(IBlockVisitor visitor);
	
	/// <summary>
	/// Format this rich text block as a HTML string.
	/// </summary>
	public string ToHtml(HtmlWritingOptions? options = null)
	{
		var writer = new HtmlWriter(options ?? new HtmlWritingOptions());
		Accept(writer);
		return writer.GetResult();
	}
	
	public virtual bool Equals(Block? other)
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
		return obj is Block other && Equals(other);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(Type);
	}

	public override string ToString() => $"Block {{ Type = {Type} }}";

	public static bool operator ==(Block? lhs, Block? rhs) => lhs?.Equals(rhs) ?? ReferenceEquals(rhs, null);
	public static bool operator !=(Block? lhs, Block? rhs) => !(lhs == rhs);
}
