using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using FluentFeeds.Documents.Inlines;

namespace FluentFeeds.Documents.Html;

/// <summary>
/// Class for transforming HTML elements to rich text inlines.
/// </summary>
internal sealed class HtmlInlineProcessor : HtmlProcessor
{
	public static ImmutableArray<Inline> TransformAll(HtmlParsingOptions options, IEnumerable<INode> nodes)
	{
		var processor = new HtmlInlineProcessor(options);
		processor.ProcessAll(nodes);
		return processor.GetResult();
	}

	private static string CollapseWhitespace(string input, bool trimStart, bool trimEnd)
	{
		var builder = new StringBuilder(capacity: input.Length);
		var isCollapsed = trimStart;
		foreach (var c in input)
		{
			if (c is ' ' or '\t' or '\n' or '\r' or '\f')
			{
				if (!isCollapsed)
					builder.Append(' ');
				isCollapsed = true;
			}
			else
			{
				isCollapsed = false;
				builder.Append(c);
			}
		}
		
		return trimEnd ? builder.ToString().TrimEnd(' ') : builder.ToString();
	}
	
	private static Uri? CreateUri(string? source)
	{
		if (String.IsNullOrEmpty(source))
			return null;
		if (!Uri.TryCreate(source, UriKind.Absolute, out var result))
			return null;
		return result;
	}

	public HtmlInlineProcessor(HtmlParsingOptions options) : base(options)
	{
	}
	
	public ImmutableArray<Inline> GetResult() => _inlines.ToImmutableArray();

	public bool HasInlines => _inlines.Count != 0;

	public void Reset()
	{
		_isFirstTextInLine = true;
		_inlines.Clear();
	}

	public override void Process(INode node)
	{
		if (node.NodeType == NodeType.Text && node is IText text)
		{
			var collapsed = CollapseWhitespace(
				text.Text, node.PreviousSibling == null || _isFirstTextInLine, node.NextSibling == null);
			if (collapsed.Length != 0)
			{
				_inlines.Add(new TextInline(collapsed));
				_isFirstTextInLine = false;
			}
		}
		else if (node.NodeType == NodeType.Element && node is IHtmlElement element)
		{
			switch (element.TagName)
			{
				case "BR":
					_inlines.Add(new TextInline("\n"));
					_isFirstTextInLine = true;
					break;
				case "IMG" when element is IHtmlImageElement imageElement:
					var width = imageElement.DisplayWidth;
					var height = imageElement.DisplayHeight;
					_inlines.Add(
						new ImageInline
						{
							Source = CreateUri(imageElement.Source),
							AlternateText = imageElement.AlternativeText,
							Width = width > 0 ? width : -1,
							Height = height > 0 ? height : -1
						});
					_isFirstTextInLine = false;
					break;
				case "B": case "STRONG":
					AddSpan(node, inlines => new BoldInline { Inlines = inlines });
					break;
				case "I": case "EM":
					AddSpan(node, inlines => new ItalicInline { Inlines = inlines });
					break;
				case "U":
					AddSpan(node, inlines => new UnderlineInline { Inlines = inlines });
					break;
				case "S":
					AddSpan(node, inlines => new StrikethroughInline { Inlines = inlines });
					break;
				case "CODE":
					AddSpan(node, inlines => new CodeInline { Inlines = inlines });
					break;
				case "A" when element is IHtmlAnchorElement anchorElement:
					AddSpan(node, inlines =>
						new HyperlinkInline { Inlines = inlines, Target = CreateUri(anchorElement.Href) });
					break;
				default:
					// Unknown tag, process inner nodes
					ProcessAll(node.ChildNodes);
					break;
			}
		}
	}

	private void AddSpan(INode node, Func<ImmutableArray<Inline>, SpanInline> createInline)
	{
		var inlines = TransformAll(Options, node.ChildNodes);
		if (inlines.Length == 0)
			return;
		_inlines.Add(createInline(inlines));
		_isFirstTextInLine = false;
	}

	private readonly List<Inline> _inlines = new();
	private bool _isFirstTextInLine = true;
}

