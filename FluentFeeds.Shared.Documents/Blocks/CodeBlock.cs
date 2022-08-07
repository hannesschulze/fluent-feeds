using System;
using System.Text.Json.Serialization;
using FluentFeeds.Shared.Documents.Json;

namespace FluentFeeds.Shared.Documents.Blocks;

/// <summary>
/// A block for formatting code with a monospace font. 
/// </summary>
[JsonConverter(typeof(BlockJsonConverter<CodeBlock>))]
public sealed class CodeBlock : Block
{
	/// <summary>
	/// Create a new default-constructed code block.
	/// </summary>
	public CodeBlock()
	{
		Code = String.Empty;
	}

	/// <summary>
	/// Create a new code block with the provided content.
	/// </summary>
	public CodeBlock(string code)
	{
		Code = code;
	}

	/// <summary>
	/// The source code.
	/// </summary>
	public string Code { get; init; }

	public override BlockType Type => BlockType.Code;

	public override void Accept(IBlockVisitor visitor) => visitor.Visit(this);
	
	public override string ToString() => $"CodeBlock {{ Code = {Code} }}";

	public override bool Equals(Block? other)
	{
		if (ReferenceEquals(this, other))
			return true;
		if (!base.Equals(other))
			return false;
		return other is CodeBlock casted && Code == casted.Code;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(base.GetHashCode(), Code);
	}
}
