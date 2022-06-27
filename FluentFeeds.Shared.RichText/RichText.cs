using System;
using System.Collections.Immutable;
using System.Linq;
using FluentFeeds.Shared.RichText.Blocks;

namespace FluentFeeds.Shared.RichText;

/// <summary>
/// <param>A collection of blocks making up rich text.</param>
///
/// <param>Like all other rich text data models, this class is immutable.</param>
/// </summary>
public sealed class RichText : IEquatable<RichText>
{
	/// <summary>
	/// Create a new default-constructed rich text object.
	/// </summary>
	public RichText()
	{
		Blocks = ImmutableArray<Block>.Empty;
	}

	/// <summary>
	/// Create a new rich text object from a list of blocks.
	/// </summary>
	public RichText(params Block[] blocks)
	{
		Blocks = ImmutableArray.Create(blocks);
	}

	/// <summary>
	/// A list of blocks making up the rich text object.
	/// </summary>
	public ImmutableArray<Block> Blocks { get; init; }

	public bool Equals(RichText? other)
	{
		if (ReferenceEquals(this, other))
			return true;
		if (ReferenceEquals(null, other))
			return false;
		return Blocks.SequenceEqual(other.Blocks);
	}

	public override bool Equals(object? obj)
	{
		if (ReferenceEquals(this, obj))
			return true;
		return obj is RichText other && Equals(other);
	}

	public override int GetHashCode()
	{
		return Blocks
			.Aggregate(new HashCode(), (hash, block) =>
			{
				hash.Add(block);
				return hash;
			})
			.ToHashCode();
	}

	public static bool operator ==(RichText lhs, RichText rhs) => lhs.Equals(rhs);
	public static bool operator !=(RichText lhs, RichText rhs) => !lhs.Equals(rhs);
}
