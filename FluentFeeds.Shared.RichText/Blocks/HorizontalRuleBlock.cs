namespace FluentFeeds.Shared.RichText.Blocks;

/// <summary>
/// Block rendered as a horizontal line.
/// </summary>
public sealed class HorizontalRuleBlock : Block
{
	/// <summary>
	/// Create a new default-constructed horizontal rule block.
	/// </summary>
	public HorizontalRuleBlock()
	{
	}

	public override BlockType Type => BlockType.HorizontalRule;

	public override void Accept(IBlockVisitor visitor) => visitor.Visit(this);

	public override bool Equals(Block? other)
	{
		if (ReferenceEquals(this, other))
			return true;
		if (!base.Equals(other))
			return false;
		return other is HorizontalRuleBlock;
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}
}
