namespace FluentFeeds.Shared.Documents.Html;

/// <summary>
/// Options for writing HTML strings.
/// </summary>
public record HtmlWritingOptions
{
	/// <summary>
	/// The number of spaces used for indentation. If this is set to a negative value, tabs are used for indentation.
	/// </summary>
	public int SpacesForIndentation { get; init; } = 2;

	/// <summary>
	/// The number of empty lines to insert between tags (not relevant for inline tags).
	/// </summary>
	public int EmptyLinesBetweenTags { get; init; } = 1;

	/// <summary>
	/// If set to <c>true</c>, empty tags (like <c>br</c>) are closed (<c>&lt;br/&gt;</c>) even if it is not strictly
	/// necessary.
	/// </summary>
	public bool CloseEmptyTags { get; init; } = true;
}
