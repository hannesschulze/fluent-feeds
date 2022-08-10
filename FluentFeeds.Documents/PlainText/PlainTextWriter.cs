using System.Runtime.CompilerServices;
using System.Text;
using FluentFeeds.Documents.Blocks;
using FluentFeeds.Documents.Inlines;

namespace FluentFeeds.Documents.PlainText;

/// <summary>
/// Visitor implementation for extracting plain text from rich text objects.
/// </summary>
public sealed class PlainTextWriter : IBlockVisitor, IInlineVisitor
{
	public string GetResult() => _builder.ToString();

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void AddWhitespace()
	{
		if (_whitespaceCollapsed)
			return;
		
		_builder.Append(' ');
		_whitespaceCollapsed = true;
	}

	private void AddText(string text)
	{
		foreach (var c in text)
		{
			if (c is ' ' or '\t' or '\n' or '\r' or '\f')
			{
				AddWhitespace();
			}
			else
			{
				_whitespaceCollapsed = false;
				_builder.Append(c);
			}
		}
	}
	
	#region IBlockVisitor
	
	public void Visit(GenericBlock block)
	{
		AddWhitespace();
		foreach (var inline in block.Inlines)
		{
			inline.Accept(this);
		}
	}

	public void Visit(ParagraphBlock block)
	{
		AddWhitespace();
		foreach (var inline in block.Inlines)
		{
			inline.Accept(this);
		}
	}

	public void Visit(HeadingBlock block)
	{
		AddWhitespace();
		foreach (var inline in block.Inlines)
		{
			inline.Accept(this);
		}
	}

	public void Visit(CodeBlock block)
	{
		AddWhitespace();
		AddText(block.Code);
	}

	public void Visit(HorizontalRuleBlock block)
	{
		AddWhitespace();
	}

	public void Visit(ListBlock block)
	{
		AddWhitespace();
		foreach (var item in block.Items)
		{
			AddWhitespace();
			foreach (var child in item.Blocks)
			{
				child.Accept(this);
			}
		}
	}

	public void Visit(QuoteBlock block)
	{
		AddWhitespace();
		foreach (var child in block.Blocks)
		{
			child.Accept(this);
		}
	}

	public void Visit(TableBlock block)
	{
		AddWhitespace();
		foreach (var row in block.Rows)
		{
			AddWhitespace();
			foreach (var cell in row.Cells)
			{
				AddWhitespace();
				foreach (var child in cell.Blocks)
				{
					child.Accept(this);
				}
			}
		}
	}

	#endregion

	#region IInlineVisitor
	
	public void Visit(TextInline inline)
	{
		AddText(inline.Text);
	}

	public void Visit(ImageInline inline)
	{
	}

	public void Visit(BoldInline inline)
	{
		VisitSpan(inline);
	}

	public void Visit(ItalicInline inline)
	{
		VisitSpan(inline);
	}

	public void Visit(UnderlineInline inline)
	{
		VisitSpan(inline);
	}

	public void Visit(StrikethroughInline inline)
	{
		VisitSpan(inline);
	}

	public void Visit(CodeInline inline)
	{
		VisitSpan(inline);
	}

	public void Visit(HyperlinkInline inline)
	{
		VisitSpan(inline);
	}

	private void VisitSpan(SpanInline inline)
	{
		foreach (var child in inline.Inlines)
		{
			child.Accept(this);
		}
	}

	#endregion

	private readonly StringBuilder _builder = new();
	private bool _whitespaceCollapsed = true;
}
