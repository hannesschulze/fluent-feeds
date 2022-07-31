using System;
using FluentFeeds.Shared.RichText.Blocks;
using FluentFeeds.Shared.RichText.Blocks.Heading;
using FluentFeeds.Shared.RichText.Html;
using FluentFeeds.Shared.RichText.Inlines;
using Xunit;

namespace FluentFeeds.Shared.RichText.Tests.Html;

public class HtmlProcessorTests
{
	[Fact]
	public void StrictMode_Enabled_Invalid()
	{
		Assert.False(RichText.TryParseHtml("<test", out var richText, new HtmlParsingOptions { IsStrict = true }));
		Assert.Equal(new RichText(), richText);
	}
	
	[Fact]
	public void StrictMode_Enabled_Valid()
	{
		Assert.True(RichText.TryParseHtml(
			"<!Doctype Html><Html><Body>Test</Body></Html>", out var richText,
			new HtmlParsingOptions { IsStrict = true }));
		Assert.Equal(new RichText(new GenericBlock(new TextInline("Test"))), richText);
	}
	[Fact]
	public void StrictMode_Disabled_Invalid()
	{
		Assert.True(RichText.TryParseHtml("<test", out var richText, new HtmlParsingOptions { IsStrict = false }));
		Assert.Equal(new RichText(), richText);
	}
	
	[Fact]
	public void StrictMode_Disabled_Valid()
	{
		Assert.True(RichText.TryParseHtml(
			"<!Doctype Html><Html><Body>Test</Body></Html>", out var richText,
			new HtmlParsingOptions { IsStrict = false }));
		Assert.Equal(new RichText(new GenericBlock(new TextInline("Test"))), richText);
	}

	[Fact]
	public void Blocks_NestedGenericBlocks()
	{
		var actual = RichText.ParseHtml("<article><div><p>test</p></div></article>");
		var expected = new RichText(new ParagraphBlock(new TextInline("test")));
		Assert.Equal(expected, actual);
	}

	[Fact]
	public void Blocks_InlinesSeparatedByGenericBlocks()
	{
		var actual = RichText.ParseHtml("foo<div>bar</div>baz");
		var expected = new RichText(
			new GenericBlock(new TextInline("foo")),
			new GenericBlock(new TextInline("bar")),
			new GenericBlock(new TextInline("baz")));
		Assert.Equal(expected, actual);
	}

	[Fact]
	public void Blocks_InlinesSeparatedByKnownBlock()
	{
		var actual = RichText.ParseHtml("<div>foo<p>bar</p>baz</div>");
		var expected = new RichText(
			new GenericBlock(new TextInline("foo")),
			new ParagraphBlock(new TextInline("bar")),
			new GenericBlock(new TextInline("baz")));
		Assert.Equal(expected, actual);
	}

	[Fact]
	public void Blocks_MultipleInlinesInGenericBlock()
	{
		var actual = RichText.ParseHtml("foo<span>bar</span>baz");
		var expected = new RichText(
			new GenericBlock(new TextInline("foo"), new TextInline("bar"), new TextInline("baz")));
		Assert.Equal(expected, actual);
	}

	[Fact]
	public void Blocks_NoInlinesInGenericBlock()
	{
		Assert.Equal(new RichText(), RichText.ParseHtml("<test/>"));
	}

	[Fact]
	public void Blocks_WhitespaceIgnored()
	{
		var actual = RichText.ParseHtml(
			"<div>\n" +
			"  <p>\n" +
			"    test\n" +
			"  </p>\n" +
			"</div>");
		var expected = new RichText(new ParagraphBlock(new TextInline("test")));
		Assert.Equal(expected, actual);
	}

	[Fact]
	public void Blocks_Known_Paragraph()
	{
		var actual = RichText.ParseHtml("<p>foo</p>");
		var expected = new RichText(new ParagraphBlock(new TextInline("foo")));
		Assert.Equal(expected, actual);
	}

	[Theory]
	[InlineData("<h1>foo</h1>", HeadingLevel.Level1)]
	[InlineData("<h2>foo</h2>", HeadingLevel.Level2)]
	[InlineData("<h3>foo</h3>", HeadingLevel.Level3)]
	[InlineData("<h4>foo</h4>", HeadingLevel.Level4)]
	[InlineData("<h5>foo</h5>", HeadingLevel.Level5)]
	[InlineData("<h6>foo</h6>", HeadingLevel.Level6)]
	public void Blocks_Known_Heading(string html, HeadingLevel headingLevel)
	{
		var actual = RichText.ParseHtml(html);
		var expected = new RichText(new HeadingBlock(new TextInline("foo")) { Level = headingLevel });
		Assert.Equal(expected, actual);
	}

	[Fact]
	public void Blocks_Known_Code_NoTrailingNewline()
	{
		var actual = RichText.ParseHtml("<pre>hello, \n\tworld</pre>");
		var expected = new RichText(new CodeBlock("hello, \n\tworld"));
		Assert.Equal(expected, actual);
	}

	[Fact]
	public void Blocks_Known_Code_TrailingNewline()
	{
		var actual = RichText.ParseHtml("<pre>\nfoo\r\n</pre>");
		var expected = new RichText(new CodeBlock("foo"));
		Assert.Equal(expected, actual);
	}

	[Fact]
	public void Blocks_Known_Code_MultipleTrailingNewlines()
	{
		var actual = RichText.ParseHtml("<pre>\nfoo\r\n\n\r\n</pre>");
		var expected = new RichText(new CodeBlock("foo\n\n"));
		Assert.Equal(expected, actual);
	}

	[Fact]
	public void Blocks_Known_HorizontalRule()
	{
		var actual = RichText.ParseHtml("foo<hr>bar");
		var expected = new RichText(
			new GenericBlock(new TextInline("foo")),
			new HorizontalRuleBlock(),
			new GenericBlock(new TextInline("bar")));
		Assert.Equal(expected, actual);
	}

	[Fact]
	public void Blocks_Known_Quote()
	{
		var actual = RichText.ParseHtml("<blockquote>foo<p>bar</p>baz</blockquote>");
		var expected = new RichText(
			new QuoteBlock(
				new GenericBlock(new TextInline("foo")), 
				new ParagraphBlock(new TextInline("bar")), 
				new GenericBlock(new TextInline("baz"))));
		Assert.Equal(expected, actual);
	}

	[Fact]
	public void Inlines_UnknownIgnored()
	{
		var actual = RichText.ParseHtml("<p>foo<test>bar</test>baz</p>");
		var expected = new RichText(
			new ParagraphBlock(new TextInline("foo"), new TextInline("bar"), new TextInline("baz")));
		Assert.Equal(expected, actual);
	}

	[Fact]
	public void Inlines_CollapseWhitespace()
	{
		var actual = RichText.ParseHtml("<p>  foo\t <span>  \n</span>\fbaz \r</p>");
		var expected = new RichText(new ParagraphBlock(new TextInline("foo "), new TextInline(" baz")));
		Assert.Equal(expected, actual);
	}
	
	[Fact]
	public void Inlines_CollapseWhitespace_Newlines()
	{
		var actual = RichText.ParseHtml("<p>foo\t <br>\n <br>  bar</p>");
		var expected = new RichText(
			new ParagraphBlock(
				new TextInline("foo "),
				new TextInline("\n"),
				new TextInline("\n"),
				new TextInline("bar")));
		Assert.Equal(expected, actual);
	}

	[Fact]
	public void Inlines_Known_Image_NoUri()
	{
		var actual = RichText.ParseHtml("<p><img/> foo</p>");
		var expected = new RichText(
			new ParagraphBlock(
				new ImageInline(),
				new TextInline(" foo")));
		Assert.Equal(expected, actual);
	}

	[Fact]
	public void Inlines_Known_Image_RelativeUri()
	{
		var actual = RichText.ParseHtml(
			"<p><img src=\"images/test.png\"/> foo</p>",
			new HtmlParsingOptions { BaseUri = new Uri("https://www.example.com") });
		var expected = new RichText(
			new ParagraphBlock(
				new ImageInline(new Uri("https://www.example.com/images/test.png")),
				new TextInline(" foo")));
		Assert.Equal(expected, actual);
	}

	[Fact]
	public void Inlines_Known_Image_AbsoluteUri()
	{
		var actual = RichText.ParseHtml("<p><img src=\"http://localhost/images/test.png\"/> foo</p>");
		var expected = new RichText(
			new ParagraphBlock(
				new ImageInline(new Uri("http://localhost/images/test.png")),
				new TextInline(" foo")));
		Assert.Equal(expected, actual);
	}

	[Fact]
	public void Inlines_Known_Image_OtherAttributes()
	{
		var actual = RichText.ParseHtml("<p><img alt=\"foo&amp;bar\" width=\"500\" height=\"600\"/> foo</p>");
		var expected = new RichText(
			new ParagraphBlock(
				new ImageInline { Width = 500, Height = 600, AlternateText = "foo&bar" },
				new TextInline(" foo")));
		Assert.Equal(expected, actual);
	}

	[Theory]
	[InlineData("<p><b> foo </b> bar</p>")]
	[InlineData("<p><strong> foo </strong> bar</p>")]
	public void Inlines_Known_Bold(string html)
	{
		var actual = RichText.ParseHtml(html);
		var expected = new RichText(
			new ParagraphBlock(
				new BoldInline(new TextInline("foo")),
				new TextInline(" bar")));
		Assert.Equal(expected, actual);
	}

	[Fact]
	public void Inlines_Known_Bold_Empty()
	{
		var actual = RichText.ParseHtml("<p><b></b> foo</p>");
		var expected = new RichText(
			new ParagraphBlock(
				new TextInline("foo")));
		Assert.Equal(expected, actual);
	}

	[Theory]
	[InlineData("<p><i> foo </i> bar</p>")]
	[InlineData("<p><em> foo </em> bar</p>")]
	public void Inlines_Known_Italic(string html)
	{
		var actual = RichText.ParseHtml(html);
		var expected = new RichText(
			new ParagraphBlock(
				new ItalicInline(new TextInline("foo")),
				new TextInline(" bar")));
		Assert.Equal(expected, actual);
	}

	[Fact]
	public void Inlines_Known_Underline()
	{
		var actual = RichText.ParseHtml("<p><u> foo </u> bar</p>");
		var expected = new RichText(
			new ParagraphBlock(
				new UnderlineInline(new TextInline("foo")),
				new TextInline(" bar")));
		Assert.Equal(expected, actual);
	}

	[Fact]
	public void Inlines_Known_Strikethrough()
	{
		var actual = RichText.ParseHtml("<p><s> foo </s> bar</p>");
		var expected = new RichText(
			new ParagraphBlock(
				new StrikethroughInline(new TextInline("foo")),
				new TextInline(" bar")));
		Assert.Equal(expected, actual);
	}

	[Fact]
	public void Inlines_Known_Code()
	{
		var actual = RichText.ParseHtml("<p><code> foo </code> bar</p>");
		var expected = new RichText(
			new ParagraphBlock(
				new CodeInline(new TextInline("foo")),
				new TextInline(" bar")));
		Assert.Equal(expected, actual);
	}

	[Fact]
	public void Inlines_Known_Hyperlink_NoUri()
	{
		var actual = RichText.ParseHtml("<p><a> foo </a> bar</p>");
		var expected = new RichText(
			new ParagraphBlock(
				new HyperlinkInline(new TextInline("foo")),
				new TextInline(" bar")));
		Assert.Equal(expected, actual);
	}

	[Fact]
	public void Inlines_Known_Hyperlink_RelativeUri()
	{
		var actual = RichText.ParseHtml(
			"<p><a href=\"images/test.png\"> foo </a> bar</p>",
			new HtmlParsingOptions { BaseUri = new Uri("https://www.example.com") });
		var expected = new RichText(
			new ParagraphBlock(
				new HyperlinkInline(
					new TextInline("foo")) { Target = new Uri("https://www.example.com/images/test.png") },
				new TextInline(" bar")));
		Assert.Equal(expected, actual);
	}

	[Fact]
	public void Inlines_Known_Hyperlink_AbsoluteUri()
	{
		var actual = RichText.ParseHtml("<p><a href=\"http://localhost/images/test.png\"> foo </a> bar</p>");
		var expected = new RichText(
			new ParagraphBlock(
				new HyperlinkInline(new TextInline("foo")) { Target = new Uri("http://localhost/images/test.png") },
				new TextInline(" bar")));
		Assert.Equal(expected, actual);
	}
}
