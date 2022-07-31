using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using FluentFeeds.Shared.RichText.Inlines;

namespace FluentFeeds.Shared.RichText.Html;

/// <summary>
/// Class for transforming HTML elements to rich text inlines.
/// </summary>
internal sealed class HtmlInlineProcessor : HtmlProcessor
{
	public static ImmutableArray<Inline> TransformAll(IEnumerable<INode> nodes)
	{
		var processor = new HtmlInlineProcessor();
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
				case "IMG":
					// TODO
					break;
				case "B": case "STRONG":
					// TODO
					break;
				case "I": case "EM":
					// TODO
					break;
				case "U":
					// TODO
					break;
				case "S":
					// TODO
					break;
				case "CODE":
					// TODO
					break;
				case "A":
					// TODO
					break;
				default:
					// Unknown tag, process inner nodes
					ProcessAll(node.ChildNodes);
					break;
			}
		}
	}

	private readonly List<Inline> _inlines = new();
	private bool _isFirstTextInLine = true;
}

