using System;
using System.Collections.Immutable;
using System.Linq;
using FluentFeeds.Shared.RichText.Blocks;

namespace FluentFeeds.Shared.RichText;

/// <summary>
/// A collection of blocks making up a rich text document.
/// </summary>
public sealed class RichText : IEquatable<RichText>
{
	public static RichText ParseHtml(string html) =>
		throw new NotImplementedException();

	public static bool TryParseHtml(string html, out RichText document)
	{
		try
		{
			document = ParseHtml(html);
			return true;
		}
		catch (Exception)
		{
			document = new RichText();
			return false;
		}
	}

	public RichText(params Block[] blocks)
	{
		Blocks = ImmutableArray.Create(blocks);
	}

	public RichText()
	{
		Blocks = ImmutableArray<Block>.Empty;
	}

	public ImmutableArray<Block> Blocks { get; init; }

	public bool Equals(RichText? other)
	{
		if (ReferenceEquals(null, other))
			return false;
		return ReferenceEquals(this, other) || Blocks.SequenceEqual(other.Blocks);
	}

	public override bool Equals(object? obj) =>
		ReferenceEquals(this, obj) || (obj is RichText other && Equals(other));

	public override int GetHashCode() =>
		Blocks
			.Aggregate(new HashCode(), (hash, block) =>
			{
				hash.Add(block);
				return hash;
			})
			.ToHashCode();
}
