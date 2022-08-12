using System;
using System.ServiceModel.Syndication;
using System.Threading.Tasks;
using FluentFeeds.Documents;
using FluentFeeds.Documents.Blocks;
using FluentFeeds.Documents.Inlines;

namespace FluentFeeds.Feeds.Syndication.Helpers;

/// <summary>
/// Abstraction for text content which directly stores the content in its representation (either plain text or rich
/// text).
/// </summary>
public abstract class TextContent
{
	private sealed class PlainTextContent : TextContent
	{
		public static PlainTextContent Empty => new(String.Empty);
		
		public PlainTextContent(string content)
		{
			_content = content;
		}

		public override RichText ToRichText() => new(new GenericBlock(new TextInline(_content)));
		public override string ToPlainText() => _content;
		
		private readonly string _content;
	}

	private sealed class RichTextContent : TextContent
	{
		public static RichTextContent Hyperlink(Uri url) =>
			new(new RichText(new GenericBlock(new HyperlinkInline(new TextInline(url.ToString())) { Target = url })));
		
		public RichTextContent(RichText content)
		{
			_content = content;
		}

		public override RichText ToRichText() => _content;
		public override string ToPlainText() => _content.ToPlainText();
		
		private readonly RichText _content;
	}
	
	/// <summary>
	/// Auto-detect if <c>content</c> is HTML content.
	/// </summary>
	public static bool IsHtml(string content) =>
		content.Contains("<p>", StringComparison.OrdinalIgnoreCase) ||
		content.Contains("<div>", StringComparison.OrdinalIgnoreCase) ||
		content.Contains("<br>", StringComparison.OrdinalIgnoreCase) ||
		content.Contains("<br/>", StringComparison.OrdinalIgnoreCase) ||
		content.Contains("<br />", StringComparison.OrdinalIgnoreCase);

	/// <summary>
	/// Create a text content object from a string, automatically detecting the content type..
	/// </summary>
	/// <param name="content">
	/// The content (either HTML or plain text).
	/// </param>
	/// <param name="isKnownHtml">
	/// If set to true, the automatic detection is skipped and <c>content</c> is treated as HTML.
	/// </param>
	public static async Task<TextContent> LoadAsync(string content, bool isKnownHtml = false) =>
		isKnownHtml || IsHtml(content)
			? new RichTextContent(await RichText.ParseHtmlAsync(content))
			: new PlainTextContent(content);

	/// <summary>
	/// Create a text content object from text syndication content.
	/// </summary>
	public static Task<TextContent> LoadAsync(TextSyndicationContent content) =>
		content.Text != null
			? LoadAsync(content.Text, content.Type is "html" or "xhtml")
			: Task.FromResult<TextContent>(PlainTextContent.Empty);
	
	/// <summary>
	/// Create a text content object from syndication content.
	/// </summary>
	public static Task<TextContent> LoadAsync(SyndicationContent content) =>
		content switch
		{
			TextSyndicationContent textContent => LoadAsync(textContent),
			UrlSyndicationContent urlContent when urlContent.Url != null =>
				Task.FromResult<TextContent>(RichTextContent.Hyperlink(urlContent.Url)),
			_ => Task.FromResult<TextContent>(PlainTextContent.Empty)
		};

	public abstract RichText ToRichText();
	public abstract string ToPlainText();
}
