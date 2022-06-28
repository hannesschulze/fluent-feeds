using FluentFeeds.Shared.RichText.Blocks;
using FluentFeeds.Shared.RichText.Blocks.Heading;
using FluentFeeds.Shared.RichText.Blocks.List;
using FluentFeeds.Shared.RichText.Inlines;
using Xunit;

namespace FluentFeeds.Shared.RichText.Tests.Html;

public class HtmlWriterTests
{
	[Fact]
	public void FormatMultipleBlocks()
	{
		var richText = new RichText(
			new ParagraphBlock(
				new TextInline("foo")),
			new ParagraphBlock(
				new TextInline("bar")));
		Assert.Equal("<p>foo</p>\n\n<p>bar</p>", richText.ToHtml());
	}

	[Fact]
	public void FormatBlock_Paragraph()
	{
		var block = new ParagraphBlock(
			new TextInline("foo"),
			new TextInline("bar"));
		Assert.Equal("<p>foobar</p>", block.ToHtml());
	}

	[Fact]
	public void FormatBlock_Code()
	{
		var block = new CodeBlock("void Foo() =>\n    Bar();");
		Assert.Equal("<pre>\nvoid Foo() =&gt;\n    Bar();\n</pre>", block.ToHtml());
	}

	[Fact]
	public void FormatBlock_Code_Nested()
	{
		var block = new QuoteBlock(
			new CodeBlock("void Foo() =>\n    Bar();"));
		Assert.Equal("<blockquote>\n<pre>\nvoid Foo() =&gt;\n    Bar();\n</pre>\n</blockquote>", block.ToHtml());
	}

	[Theory]
	[InlineData(HeadingLevel.Level1, "<h1>foobar</h1>")]
	[InlineData(HeadingLevel.Level2, "<h2>foobar</h2>")]
	[InlineData(HeadingLevel.Level3, "<h3>foobar</h3>")]
	[InlineData(HeadingLevel.Level4, "<h4>foobar</h4>")]
	[InlineData(HeadingLevel.Level5, "<h5>foobar</h5>")]
	[InlineData(HeadingLevel.Level6, "<h6>foobar</h6>")]
	public void FormatBlock_Heading(HeadingLevel level, string expectedResult)
	{
		var block = new HeadingBlock(
			new TextInline("foo"),
			new TextInline("bar")) {Level = level};
		Assert.Equal(expectedResult, block.ToHtml());
	}

	[Fact]
	public void FormatBlock_HorizontalRule()
	{
		var block = new HorizontalRuleBlock();
		Assert.Equal("<hr/>", block.ToHtml());
	}

	[Fact]
	public void FormatBlock_List_LeafItems()
	{
		var block = new ListBlock(
			new LeafListItem(
				new TextInline("foo"),
				new TextInline("bar")),
			new LeafListItem(
				new TextInline("baz")));
		Assert.Equal("<ul>\n  <li>foobar</li>\n\n  <li>baz</li>\n</ul>", block.ToHtml());
	}

	[Theory]
	[InlineData(
		ListStyle.Unordered, ListStyle.Unordered, 
		"<ul>\n  <li>\n    <ul>\n      <li>foo</li>\n\n      <li>bar</li>\n    </ul>\n  </li>\n</ul>")]
	[InlineData(
		ListStyle.Unordered, ListStyle.Ordered, 
		"<ul>\n  <li>\n    <ol>\n      <li>foo</li>\n\n      <li>bar</li>\n    </ol>\n  </li>\n</ul>")]
	[InlineData(
		ListStyle.Ordered, ListStyle.Unordered, 
		"<ol>\n  <li>\n    <ul>\n      <li>foo</li>\n\n      <li>bar</li>\n    </ul>\n  </li>\n</ol>")]
	[InlineData(
		ListStyle.Ordered, ListStyle.Ordered, 
		"<ol>\n  <li>\n    <ol>\n      <li>foo</li>\n\n      <li>bar</li>\n    </ol>\n  </li>\n</ol>")]
	public void FormatBlock_List_NestedItems(ListStyle outerStyle, ListStyle innerStyle, string expectedResult)
	{
		var block = new ListBlock(
			new NestedListItem(
				new LeafListItem(
					new TextInline("foo")),
				new LeafListItem(
					new TextInline("bar"))) {Style = innerStyle}) {Style = outerStyle};
		Assert.Equal(expectedResult, block.ToHtml());
	}

	[Fact]
	public void FormatBlock_Quote()
	{
		var block = new QuoteBlock(
			new ParagraphBlock(
				new TextInline("foo")),
			new ParagraphBlock(
				new TextInline("bar")));
		Assert.Equal("<blockquote>\n  <p>foo</p>\n\n  <p>bar</p>\n</blockquote>", block.ToHtml());
	}

	[Fact]
	public void FormatInline()
	{
		var inline = new TextInline("foo&bar");
		Assert.Equal("foo&amp;bar", inline.ToHtml());
	}
}
