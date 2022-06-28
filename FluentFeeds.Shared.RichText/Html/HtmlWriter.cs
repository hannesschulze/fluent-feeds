using System;
using System.Collections.Generic;
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
	
	public void Visit(ImageInline inline)
	{
		var attributes = new Dictionary<string, string>();
		if (inline.Source != null)
			attributes.Add("src", inline.Source.ToString());
		if (inline.AlternateText != null)
			attributes.Add("alt", inline.AlternateText);
		if (inline.Width >= 0)
			attributes.Add("width", inline.Width.ToString());
		if (inline.Height >= 0)
			attributes.Add("height", inline.Height.ToString());
		
		_builder.AppendEmptyTag("img", attributes);
	}

	public void Visit(BoldInline inline)
	{
		WrapSpan(inline, "b");
	}

	public void Visit(ItalicInline inline)
	{
		WrapSpan(inline, "i");
	}

	public void Visit(UnderlineInline inline)
	{
		WrapSpan(inline, "u");
	}

	public void Visit(StrikethroughInline inline)
	{
		WrapSpan(inline, "s");
	}

	public void Visit(CodeInline inline)
	{
		WrapSpan(inline, "code");
	}

	public void Visit(SuperscriptInline inline)
	{
		WrapSpan(inline, "sup");
	}

	public void Visit(SubscriptInline inline)
	{
		WrapSpan(inline, "sub");
	}

	public void Visit(HyperlinkInline inline)
	{
		var attributes = new Dictionary<string, string>();
		if (inline.Target != null)
			attributes.Add("href", inline.Target.ToString());
		
		WrapSpan(inline, "a", attributes);
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
		_builder.AppendTagOpen("li", false);
		foreach (var inline in listItem.Content.Inlines)
				inline.Accept(this);
		_builder.AppendTagOpen(TagForListStyle(listItem.Style), false);
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

	private void WrapSpan(SpanInline span, string tag, IReadOnlyDictionary<string, string>? attributes = null)
	{
		_builder.AppendTagOpen(tag, true, attributes);
		foreach (var inline in span.Inlines)
			inline.Accept(this);
		_builder.AppendTagClose();
	}

	#endregion
	
	private readonly HtmlBuilder _builder;
}
