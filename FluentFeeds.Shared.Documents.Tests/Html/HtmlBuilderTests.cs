using System.Collections.Generic;
using FluentFeeds.Shared.Documents.Html;
using Xunit;

namespace FluentFeeds.Shared.Documents.Tests.Html;

public class HtmlBuilderTests
{
	[Fact]
	public void LineBreaks_RootScope_Empty()
	{
		var builder = new HtmlBuilder(new HtmlWritingOptions());
		Assert.Equal("", builder.GetResult());
	}
	
	[Fact]
	public void LineBreaks_RootScope_ManyElements()
	{
		var builder = new HtmlBuilder(new HtmlWritingOptions());
		builder.AppendEmptyTag("foo");
		builder.AppendEmptyTag("bar");
		Assert.Equal("<foo/>\n\n<bar/>", builder.GetResult());
	}

	[Theory]
	[InlineData(false, "<foo>\n</foo>")]
	[InlineData(true, "<foo></foo>")]
	public void LineBreaks_NestedScope_Empty(bool inline, string expectedResult)
	{
		var builder = new HtmlBuilder(new HtmlWritingOptions());
		builder.AppendTagOpen("foo", inline);
		builder.AppendTagClose();
		Assert.Equal(expectedResult, builder.GetResult());
	}

	[Theory]
	[InlineData(false, "<foo>\n  <bar/>\n\n  <baz/>\n</foo>")]
	[InlineData(true, "<foo><bar/><baz/></foo>")]
	public void LineBreaks_NestedScope_ManyElements(bool inline, string expectedResult)
	{
		var builder = new HtmlBuilder(new HtmlWritingOptions());
		builder.AppendTagOpen("foo", inline);
		builder.AppendEmptyTag("bar");
		builder.AppendEmptyTag("baz");
		builder.AppendTagClose();
		Assert.Equal(expectedResult, builder.GetResult());
	}
	
	[Theory]
	[InlineData(0, "<foo/>\n<bar/>")]
	[InlineData(2, "<foo/>\n\n\n<bar/>")]
	[InlineData(-1, "<foo/>\n<bar/>")]
	public void LineBreaks_CustomEmptyLines(int emptyLinesBetweenTags, string expectedResult)
	{
		var builder = new HtmlBuilder(new HtmlWritingOptions {EmptyLinesBetweenTags = emptyLinesBetweenTags});
		builder.AppendEmptyTag("foo");
		builder.AppendEmptyTag("bar");
		Assert.Equal(expectedResult, builder.GetResult());
	}

	[Fact]
	public void Indentation_DisableTemporarily()
	{
		var builder = new HtmlBuilder(new HtmlWritingOptions());
		builder.AppendTagOpen("foo", false);
		builder.AppendTagOpen("bar", false);
		builder.DisableIndentation = true;
		builder.AppendEmptyTag("baz");
		builder.AppendTagClose();
		builder.DisableIndentation = false;
		builder.AppendTagClose();
		Assert.Equal("<foo>\n  <bar>\n<baz/>\n</bar>\n</foo>", builder.GetResult());
	}

	[Theory]
	[InlineData(0, "<foo>\n<bar>\n<baz/>\n</bar>\n</foo>")]
	[InlineData(4, "<foo>\n    <bar>\n        <baz/>\n    </bar>\n</foo>")]
	[InlineData(-1, "<foo>\n\t<bar>\n\t\t<baz/>\n\t</bar>\n</foo>")]
	public void Indentation_CustomSetting(int spacesForIndentation, string expectedResult)
	{
		var builder = new HtmlBuilder(new HtmlWritingOptions {SpacesForIndentation = spacesForIndentation});
		builder.AppendTagOpen("foo", false);
		builder.AppendTagOpen("bar", false);
		builder.AppendEmptyTag("baz");
		builder.AppendTagClose();
		builder.AppendTagClose();
		Assert.Equal(expectedResult, builder.GetResult());
	}

	[Theory]
	[InlineData(false, "<foo>")]
	[InlineData(true, "<foo/>")]
	public void Elements_Tag_Empty(bool close, string expectedResult)
	{
		var builder = new HtmlBuilder(new HtmlWritingOptions {CloseEmptyTags = close});
		builder.AppendEmptyTag("foo");
		Assert.Equal(expectedResult, builder.GetResult());
	}

	[Fact]
	public void Elements_Tag_CloseInRootScope()
	{
		var builder = new HtmlBuilder(new HtmlWritingOptions());
		builder.AppendTagClose();
		builder.AppendTagOpen("foo", true);
		builder.AppendTagClose();
		builder.AppendTagClose();
		Assert.Equal("<foo></foo>", builder.GetResult());
	}

	[Fact]
	public void Elements_Tag_Attributes()
	{
		var builder = new HtmlBuilder(new HtmlWritingOptions());
		builder.AppendEmptyTag(
			"foo", new Dictionary<string, string> {{"attr1", "bar"}, {"attr2", "baz"}});
		Assert.Equal("<foo attr1=\"bar\" attr2=\"baz\"/>", builder.GetResult());
	}

	[Fact]
	public void Elements_Tag_Attributes_EscapeValues()
	{
		var builder = new HtmlBuilder(new HtmlWritingOptions());
		builder.AppendEmptyTag(
			"foo", new Dictionary<string, string> {{"attr", "\r\nabc&<>'\""}});
		Assert.Equal("<foo attr=\"&#10;abc&amp;&lt;&gt;&apos;&quot;\"/>", builder.GetResult());
	}

	[Fact]
	public void Elements_Text_EscapeSpecialCharacters()
	{
		var builder = new HtmlBuilder(new HtmlWritingOptions());
		builder.AppendTagOpen("p", true);
		builder.AppendText("\r\"abc&<>'");
		builder.AppendTagClose();
		Assert.Equal("<p>\"abc&amp;&lt;&gt;'</p>", builder.GetResult());
	}

	[Theory]
	[InlineData(false, false, "<p>\n  foo\n  bar\n</p>")]
	[InlineData(false, true, "<p>\n  foo<br/>\n  bar\n</p>")]
	[InlineData(true, false, "<p>foo&#10;bar</p>")]
	[InlineData(true, true, "<p>foo<br/>bar</p>")]
	public void Elements_Text_LineBreaks(bool inline, bool transformLineBreaks, string expectedResult)
	{
		var builder = new HtmlBuilder(new HtmlWritingOptions());
		builder.AppendTagOpen("p", inline);
		builder.AppendText("foo\nbar", transformLineBreaks);
		builder.AppendTagClose();
		Assert.Equal(expectedResult, builder.GetResult());
	}
}
