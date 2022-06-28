using System;
using FluentFeeds.Shared.RichText.Blocks;
using FluentFeeds.Shared.RichText.Blocks.Heading;
using FluentFeeds.Shared.RichText.Blocks.List;
using FluentFeeds.Shared.RichText.Inlines;

namespace FluentFeeds.Shared.RichText.Html;

/// <summary>
/// Visitor implementation for building HTML.
/// </summary>
internal sealed class HtmlWriter : IBlockVisitor, IInlineVisitor, IListItemVisitor
{
	public HtmlWriter(HtmlWritingOptions options)
	{
		_builder = new HtmlBuilder(options);
	}

	public string GetResult() => _builder.GetResult();

	#region IBlockVisitor
	
	public void Visit(ParagraphBlock block)
	{
		_builder.AppendTagOpen("p", true);
		foreach (var inline in block.Inlines)
			inline.Accept(this);
		_builder.AppendTagClose();
	}

	public void Visit(CodeBlock block)
	{
		_builder.DisableIndentation = true;
		_builder.AppendTagOpen("pre", false).AppendText(block.Code, transformLineBreaks: false).AppendTagClose();
		_builder.DisableIndentation = false;
	}

	public void Visit(HeadingBlock block)
	{
		_builder.AppendTagOpen(TagForHeadingLevel(block.Level), true);
		foreach (var inline in block.Inlines)
			inline.Accept(this);
		_builder.AppendTagClose();
	}

	public void Visit(HorizontalRuleBlock block)
	{
		_builder.AppendEmptyTag("hr");
	}

	public void Visit(ListBlock block)
	{
		_builder.AppendTagOpen(TagForListStyle(block.Style), false);
		foreach (var item in block.Items)
			item.Accept(this);
		_builder.AppendTagClose();
	}

	public void Visit(QuoteBlock block)
	{
		_builder.AppendTagOpen("blockquote", false);
		foreach (var quoteBlock in block.Blocks)
			quoteBlock.Accept(this);
		_builder.AppendTagClose();
	}
	
	#endregion
	
	#region IInlineVisitor

	public void Visit(TextInline inline)
	{
		_builder.AppendText(inline.Text);
	}
	
	#endregion
	
	#region IListItemVisitor

	public void Visit(LeafListItem listItem)
	{
		_builder.AppendTagOpen("li", true);
		foreach (var inline in listItem.Inlines)
			inline.Accept(this);
		_builder.AppendTagClose();
	}
	
	public void Visit(NestedListItem listItem)
	{
		_builder.AppendTagOpen("li", false).AppendTagOpen(TagForListStyle(listItem.Style), false);
		foreach (var item in listItem.Items)
			item.Accept(this);
		_builder.AppendTagClose().AppendTagClose();
	}

	#endregion
	
	#region Helpers

	private static string TagForHeadingLevel(HeadingLevel level) =>
		level switch
		{
			HeadingLevel.Level1 => "h1",
			HeadingLevel.Level2 => "h2",
			HeadingLevel.Level3 => "h3",
			HeadingLevel.Level4 => "h4",
			HeadingLevel.Level5 => "h5",
			HeadingLevel.Level6 => "h6",
			_ => throw new IndexOutOfRangeException()
		};

	private static string TagForListStyle(ListStyle style) =>
		style switch
		{
			ListStyle.Ordered => "ol",
			ListStyle.Unordered => "ul",
			_ => throw new IndexOutOfRangeException()
		};

	#endregion
	
	private readonly HtmlBuilder _builder;
}