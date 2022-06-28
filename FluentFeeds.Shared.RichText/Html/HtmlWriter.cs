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
internal sealed class HtmlWriter : IBlockVisitor, IInlineVisitor
{
	public HtmlWriter(HtmlWritingOptions options)
	{
		_builder = new HtmlBuilder(options);
	}

	public string GetResult() => _builder.GetResult();
	
	private void WrapInlines(
		IEnumerable<Inline> inlines, string tag, IReadOnlyDictionary<string, string>? attributes = null)
	{
		_builder.AppendTagOpen(tag, true, attributes);
		foreach (var inline in inlines)
			inline.Accept(this);
		_builder.AppendTagClose();
	}

	#region IBlockVisitor

	public void Visit(GenericBlock block)
	{
		WrapInlines(block.Inlines, "div");
	}

	public void Visit(ParagraphBlock block)
	{
		WrapInlines(block.Inlines, "p");
	}
	
	public void Visit(HeadingBlock block)
	{
		WrapInlines(
			block.Inlines,
			block.Level switch
			{
				HeadingLevel.Level1 => "h1",
				HeadingLevel.Level2 => "h2",
				HeadingLevel.Level3 => "h3",
				HeadingLevel.Level4 => "h4",
				HeadingLevel.Level5 => "h5",
				HeadingLevel.Level6 => "h6",
				_ => throw new IndexOutOfRangeException()
			});
	}

	public void Visit(CodeBlock block)
	{
		_builder.DisableIndentation = true;
		_builder.AppendTagOpen("pre", false).AppendText(block.Code, transformLineBreaks: false).AppendTagClose();
		_builder.DisableIndentation = false;
	}

	public void Visit(HorizontalRuleBlock block)
	{
		_builder.AppendEmptyTag("hr");
	}

	public void Visit(ListBlock block)
	{
		_builder.AppendTagOpen(
			block.Style switch
			{
				ListStyle.Ordered => "ol",
				ListStyle.Unordered => "ul",
				_ => throw new IndexOutOfRangeException()
			}, false);
		foreach (var item in block.Items)
		{
			_builder.AppendTagOpen("li", false);
			foreach (var itemBlock in item.Blocks)
				itemBlock.Accept(this);
			_builder.AppendTagClose();
		}
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
		WrapInlines(inline.Inlines, "b");
	}

	public void Visit(ItalicInline inline)
	{
		WrapInlines(inline.Inlines, "i");
	}

	public void Visit(UnderlineInline inline)
	{
		WrapInlines(inline.Inlines, "u");
	}

	public void Visit(StrikethroughInline inline)
	{
		WrapInlines(inline.Inlines, "s");
	}

	public void Visit(CodeInline inline)
	{
		WrapInlines(inline.Inlines, "code");
	}

	public void Visit(SuperscriptInline inline)
	{
		WrapInlines(inline.Inlines, "sup");
	}

	public void Visit(SubscriptInline inline)
	{
		WrapInlines(inline.Inlines, "sub");
	}

	public void Visit(HyperlinkInline inline)
	{
		var attributes = new Dictionary<string, string>();
		if (inline.Target != null)
			attributes.Add("href", inline.Target.ToString());
		
		WrapInlines(inline.Inlines, "a", attributes);
	}

	#endregion
	
	private readonly HtmlBuilder _builder;
}
