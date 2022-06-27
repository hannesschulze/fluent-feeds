using System;

namespace FluentFeeds.Shared.RichText.Blocks;

/// <summary>
/// <para>Base class of rich text blocks.</para>
///
/// <para>Rich text objects are made up of multiple blocks. There are multiple types of blocks which can be used to
/// define a document – for example, paragraph blocks containing text (in the form of inlines) or image blocks.</para>
/// </summary>
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

	public static bool operator ==(Block lhs, Block rhs) => lhs.Equals(rhs);
	public static bool operator !=(Block lhs, Block rhs) => !lhs.Equals(rhs);
}
