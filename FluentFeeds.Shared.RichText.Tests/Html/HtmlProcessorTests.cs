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
}
