using System;
using FluentFeeds.Documents.Blocks;
using FluentFeeds.Documents.Blocks.Heading;
using FluentFeeds.Documents.Blocks.List;
using FluentFeeds.Documents.Blocks.Table;
using FluentFeeds.Documents.Inlines;
using Xunit;

namespace FluentFeeds.Documents.Tests.Html;

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
	public void FormatBlock_Generic()
	{
		var block = new GenericBlock(
			new TextInline("foo"),
			new TextInline("bar"));
		Assert.Equal("<div>foobar</div>", block.ToHtml());
	}

	[Fact]
	public void FormatBlock_Paragraph()
	{
		var block = new ParagraphBlock(
			new TextInline("foo"),
			new TextInline("bar"));
		Assert.Equal("<p>foobar</p>", block.ToHtml());
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
			new ListItem(
				new TextInline("foo"),
				new TextInline("bar")),
			new ListItem(
				new TextInline("baz")));
		Assert.Equal(
			"<ul>\n  <li>\n    <div>foobar</div>\n  </li>\n\n  <li>\n    <div>baz</div>\n  </li>\n</ul>",
			block.ToHtml());
	}

	[Theory]
	[InlineData(
		ListStyle.Unordered, ListStyle.Unordered, 
		"<ul>\n  <li>\n    <div>foo</div>\n\n    <ul>\n      <li>\n        <div>bar</div>\n      </li>\n    </ul>\n  </li>\n</ul>")]
	[InlineData(
		ListStyle.Unordered, ListStyle.Ordered, 
		"<ul>\n  <li>\n    <div>foo</div>\n\n    <ol>\n      <li>\n        <div>bar</div>\n      </li>\n    </ol>\n  </li>\n</ul>")]
	[InlineData(
		ListStyle.Ordered, ListStyle.Unordered, 
		"<ol>\n  <li>\n    <div>foo</div>\n\n    <ul>\n      <li>\n        <div>bar</div>\n      </li>\n    </ul>\n  </li>\n</ol>")]
	[InlineData(
		ListStyle.Ordered, ListStyle.Ordered, 
		"<ol>\n  <li>\n    <div>foo</div>\n\n    <ol>\n      <li>\n        <div>bar</div>\n      </li>\n    </ol>\n  </li>\n</ol>")]
	public void FormatBlock_List_NestedList(ListStyle outerStyle, ListStyle innerStyle, string expectedResult)
	{
		var block = new ListBlock(
			new ListItem(
				new GenericBlock(
					new TextInline("foo")),
				new ListBlock(
					new ListItem(
						new TextInline("bar"))) {Style = innerStyle})) {Style = outerStyle};
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
	public void FormatBlock_Table()
	{
		var block = new TableBlock(
			new TableRow(
				new TableCell(
					new TextInline("foo"))),
			new TableRow(
				new TableCell(
					new TextInline("bar")),
				new TableCell(
					new TextInline("baz"))));
		Assert.Equal(
			"<table>\n  <tr>\n    <td>\n      <div>foo</div>\n    </td>\n  </tr>\n\n  " +
			"<tr>\n    <td>\n      <div>bar</div>\n    </td>\n\n    <td>\n      <div>baz</div>\n    </td>\n  </tr>\n" +
			"</table>", block.ToHtml());
	}

	[Theory]
	[InlineData(1, 1, true, "<table>\n  <tr>\n    <th>\n      <div>foo</div>\n    </th>\n  </tr>\n</table>")]
	[InlineData(
		2, -1, false, "<table>\n  <tr>\n    <td colspan=\"2\">\n      <div>foo</div>\n    </td>\n  </tr>\n</table>")]
	[InlineData(
		-1, 3, false, "<table>\n  <tr>\n    <td rowspan=\"3\">\n      <div>foo</div>\n    </td>\n  </tr>\n</table>")]
	[InlineData(
		3, 2, true,
		"<table>\n  <tr>\n    <th colspan=\"3\" rowspan=\"2\">\n      <div>foo</div>\n    </th>\n  </tr>\n</table>")]
	public void FormatBlock_Table_CellProperties(int columnSpan, int rowSpan, bool isHeader, string expectedResult)
	{
		var block = new TableBlock(
			new TableRow(
				new TableCell(
					new TextInline("foo")) {ColumnSpan = columnSpan, RowSpan = rowSpan, IsHeader = isHeader}));
		Assert.Equal(expectedResult, block.ToHtml());
	}

	[Fact]
	public void FormatInline_Text()
	{
		var inline = new TextInline("foo&bar");
		Assert.Equal("foo&amp;bar", inline.ToHtml());
	}

	[Theory]
	[InlineData(null, null, -1, -1, "<img/>")]
	[InlineData("https://www.example.com/", null, -1, -1, "<img src=\"https://www.example.com/\"/>")]
	[InlineData("https://www.example.com/", null, 10, -1, "<img src=\"https://www.example.com/\" width=\"10\"/>")]
	[InlineData(
		"https://www.example.com/", "alternate text", -1, 20, 
		"<img src=\"https://www.example.com/\" alt=\"alternate text\" height=\"20\"/>")]
	[InlineData(
		"https://www.example.com/", "alternate text", 10, 20, 
		"<img src=\"https://www.example.com/\" alt=\"alternate text\" width=\"10\" height=\"20\"/>")]
	public void FormatInline_Image(string? source, string? alternateText, int width, int height, string expectedResult)
	{
		var inline =
			new ImageInline(source != null ? new Uri(source) : null)
			{
				AlternateText = alternateText, 
				Width = width,
				Height = height
			};
		Assert.Equal(expectedResult, inline.ToHtml());
	}

	[Fact]
	public void FormatInline_Bold()
	{
		var inline = new BoldInline(
			new TextInline("foo"),
			new TextInline("bar"));
		Assert.Equal("<b>foobar</b>", inline.ToHtml());
	}

	[Fact]
	public void FormatInline_Italic()
	{
		var inline = new ItalicInline(
			new TextInline("foo"),
			new TextInline("bar"));
		Assert.Equal("<i>foobar</i>", inline.ToHtml());
	}

	[Fact]
	public void FormatInline_Underline()
	{
		var inline = new UnderlineInline(
			new TextInline("foo"),
			new TextInline("bar"));
		Assert.Equal("<u>foobar</u>", inline.ToHtml());
	}

	[Fact]
	public void FormatInline_Strikethrough()
	{
		var inline = new StrikethroughInline(
			new TextInline("foo"),
			new TextInline("bar"));
		Assert.Equal("<s>foobar</s>", inline.ToHtml());
	}

	[Fact]
	public void FormatInline_Code()
	{
		var inline = new CodeInline(
			new TextInline("foo"),
			new TextInline("bar"));
		Assert.Equal("<code>foobar</code>", inline.ToHtml());
	}

	[Theory]
	[InlineData(null, "<a>foobar</a>")]
	[InlineData("https://www.example.com/", "<a href=\"https://www.example.com/\">foobar</a>")]
	public void FormatInline_Hyperlink(string? target, string expectedResult)
	{
		var inline = new HyperlinkInline(
			new TextInline("foo"),
			new TextInline("bar")) {Target = target != null ? new Uri(target) : null};
		Assert.Equal(expectedResult, inline.ToHtml());
	}
}
