using System.Collections.Generic;
using System.Collections.Immutable;
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
	
	public ImmutableArray<Inline> GetResult()
	{
		return _inlines.ToImmutableArray();
	}

	public bool HasInlines => _inlines.Count != 0;

	public void Reset() => _inlines.Clear();

	public override void Process(INode node)
	{
		if (node.NodeType == NodeType.Text && node is IText text)
		{
			// TODO: Collapse whitespace
			_inlines.Add(new TextInline(text.Text));
		}
		else if (node.NodeType == NodeType.Element && node is IHtmlElement element)
		{
			switch (element.TagName)
			{
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
}

