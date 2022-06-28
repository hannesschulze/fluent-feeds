namespace FluentFeeds.Shared.RichText.Blocks;

/// <summary>
/// The type of a <see cref="Block"/>.
/// </summary>
public enum BlockType
{
	/// <summary>
	/// <see cref="ParagraphBlock"/>
	/// </summary>
	Paragraph,
	/// <summary>
	/// <see cref="CodeBlock"/>
	/// </summary>
	Code,
	/// <summary>
	/// <see cref="HeadingBlock"/>
	/// </summary>
	Heading,
	/// <summary>
	/// <see cref="HorizontalRuleBlock"/>
	/// </summary>
	HorizontalRule,
	/// <summary>
	/// <see cref="ListBlock"/>
	/// </summary>
	List,
	/// <summary>
	/// <see cref="QuoteBlock"/>
	/// </summary>
	Quote
}