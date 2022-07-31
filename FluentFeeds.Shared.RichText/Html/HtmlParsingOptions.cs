using System;

namespace FluentFeeds.Shared.RichText.Html;

/// <summary>
/// Options for parsing HTML strings.
/// </summary>
public record HtmlParsingOptions
{
	/// <summary>
	/// If set to true, exceptions are thrown even if the parser could continue.
	/// </summary>
	public bool IsStrict { get; init; } = false;

	/// <summary>
	/// Base URI used for resolving resources.
	/// </summary>
	public Uri? BaseUri { get; init; } = new("about:///");
}
