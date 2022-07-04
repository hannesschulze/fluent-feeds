namespace FluentFeeds.Shared.RichText.Inlines;

/// <summary>
/// The type of an <see cref="Inline"/> element.
/// </summary>
public enum InlineType
{
	/// <summary>
	/// <see cref="TextInline"/>
	/// </summary>
	Text,
	/// <summary>
	/// <see cref="ImageInline"/>
	/// </summary>
	Image,
	/// <summary>
	/// <see cref="BoldInline"/>
	/// </summary>
	Bold,
	/// <summary>
	/// <see cref="ItalicInline"/>
	/// </summary>
	Italic,
	/// <summary>
	/// <see cref="UnderlineInline"/>
	/// </summary>
	Underline,
	/// <summary>
	/// <see cref="StrikethroughInline"/>
	/// </summary>
	Strikethrough,
	/// <summary>
	/// <see cref="CodeInline"/>
	/// </summary>
	Code,
	/// <summary>
	/// <see cref="HyperlinkInline"/>
	/// </summary>
	Hyperlink
}
