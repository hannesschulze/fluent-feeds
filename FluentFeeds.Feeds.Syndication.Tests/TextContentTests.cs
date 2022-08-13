using System;
using System.ServiceModel.Syndication;
using System.Threading.Tasks;
using System.Xml;
using FluentFeeds.Documents;
using FluentFeeds.Documents.Blocks;
using FluentFeeds.Documents.Html;
using FluentFeeds.Documents.Inlines;
using FluentFeeds.Feeds.Syndication.Helpers;
using Xunit;

namespace FluentFeeds.Feeds.Syndication.Tests;

public class TextContentTests
{
	private sealed class CustomContent : SyndicationContent
	{
		public override SyndicationContent Clone() => new CustomContent();

		protected override void WriteContentsTo(XmlWriter writer) => writer.WriteString("custom");

		public override string Type => "custom";
	}
	
	[Fact]
	public async Task CreateFromTextContent_UnknownType_PlainText()
	{
		var source = new TextSyndicationContent("hello,\n <world!>", TextSyndicationContentKind.Plaintext);
		var content = await TextContent.LoadAsync(source as SyndicationContent, new HtmlParsingOptions());
		Assert.Equal("hello,\n <world!>", content.ToPlainText());
	}
	
	[Fact]
	public async Task CreateFromTextContent_UnknownType_Html()
	{
		var source = new TextSyndicationContent("hello,<P> &lt;world!&gt</P>", TextSyndicationContentKind.Plaintext);
		var content = await TextContent.LoadAsync(source as SyndicationContent, new HtmlParsingOptions());
		var expected = new RichText(
			new GenericBlock(new TextInline("hello,")),
			new ParagraphBlock(new TextInline("<world!>")));
		Assert.Equal(expected, content.ToRichText());
	}

	[Fact]
	public async Task CreateFromTextContent_KnownHtml()
	{
		var source = new TextSyndicationContent("hello,\n <world!>", TextSyndicationContentKind.Html);
		var content = await TextContent.LoadAsync(source as SyndicationContent, new HtmlParsingOptions());
		Assert.Equal(new RichText(new GenericBlock(new TextInline("hello, "))), content.ToRichText());
	}
	
	[Fact]
	public async Task CreateFromTextContent_MissingText()
	{
		var source = new TextSyndicationContent(null, TextSyndicationContentKind.Plaintext);
		var content = await TextContent.LoadAsync(source as SyndicationContent, new HtmlParsingOptions());
		Assert.Empty(content.ToPlainText());
	}
	
	[Fact]
	public async Task CreateFromUrlContent()
	{
		var source = new UrlSyndicationContent(new Uri("https://www.example.com/"), "text/html");
		var content = await TextContent.LoadAsync(source as SyndicationContent, new HtmlParsingOptions());
		var expected = new RichText(new GenericBlock(
			new HyperlinkInline(new TextInline("https://www.example.com/"))
			{
				Target = new Uri("https://www.example.com/")
			}));
		Assert.Equal(expected, content.ToRichText());
	}
	
	[Fact]
	public async Task CreateFromUnknownContent()
	{
		var source = new CustomContent();
		var content = await TextContent.LoadAsync(source as SyndicationContent, new HtmlParsingOptions());
		Assert.Empty(content.ToPlainText());
	}

	[Fact]
	public async Task Conversion_PlainTextToRichText()
	{
		var content = await TextContent.LoadAsync("hello,\n <world!>", new HtmlParsingOptions());
		var expected = new RichText(new GenericBlock(new TextInline("hello,\n <world!>")));
		Assert.Equal(expected, content.ToRichText());
	}

	[Fact]
	public async Task Conversion_RichTextToPlainText()
	{
		var content = await TextContent.LoadAsync("hello,<P> &lt;world!&gt</P>", new HtmlParsingOptions());
		Assert.Equal("hello, <world!>", content.ToPlainText());
	}
}
