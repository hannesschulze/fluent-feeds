using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using FluentFeeds.Shared.RichText.Blocks;
using FluentFeeds.Shared.RichText.Blocks.Heading;

namespace FluentFeeds.Shared.RichText.Html;

/// <summary>
/// Class for transforming HTML elements to rich text blocks.
/// </summary>
internal sealed class HtmlBlockProcessor : HtmlProcessor
{
	public static ImmutableArray<Block> TransformAll(IEnumerable<INode> nodes)
	{
		var processor = new HtmlBlockProcessor();
		processor.ProcessAll(nodes);
		return processor.GetResult();
	}
	
	private static HeadingLevel HeadingLevelFromTagName(string tagName) =>
		tagName switch
		{
			"H1" => HeadingLevel.Level1,
			"H2" => HeadingLevel.Level2,
			"H3" => HeadingLevel.Level3,
			"H4" => HeadingLevel.Level4,
			"H5" => HeadingLevel.Level5,
			"H6" => HeadingLevel.Level6,
			_ => throw new ArgumentOutOfRangeException(nameof(tagName))
		};
	
	// https://developer.mozilla.org/en-US/docs/Web/HTML/Block-level_elements
	private static bool IsBlockTag(string tagName) =>
		tagName is "ADDRESS" or "ARTICLE" or "ASIDE" or "BLOCKQUOTE" or "DETAILS" or "DIALOG" or "DD" or "DIV" or "DL"
			or "DT" or "FIELDSET" or "FIGCAPTION" or "FIGURE" or "FOOTER" or "FORM" or "H1" or "H2" or "H3" or "H4"
			or "H5" or "H6" or "HEADER" or "HGROUP" or "HR" or "LI" or "MAIN" or "NAV" or "OL" or "P" or "PRE"
			or "SECTION" or "TABLE" or "UL";

	private static string CodeTextContent(INode node)
	{
		// Ignore nested tags (not supported)
		var result = node.TextContent;
		// Remove trailing newline (\r\n is already handled by the HTML parser)
		return result.EndsWith('\n') ? result[..^1] : result;
	}

	public ImmutableArray<Block> GetResult()
	{
		FlushGenericBlock();
		return _blocks.ToImmutableArray();
	}

	public override void Process(INode node)
	{
		var element = node as IHtmlElement;
		var tagName = element?.TagName;
		
		switch (tagName)
		{
			case "P":
				AddBlock(new ParagraphBlock { Inlines = HtmlInlineProcessor.TransformAll(node.ChildNodes) });
				break;
			case "H1": case "H2": case "H3": case "H4": case "H5": case "H6":
				AddBlock(
					new HeadingBlock
					{
						Inlines = HtmlInlineProcessor.TransformAll(node.ChildNodes),
						Level = HeadingLevelFromTagName(tagName)
					});
				break;
			case "PRE":
				AddBlock(new CodeBlock(CodeTextContent(node)));
				break;
			case "HR":
				AddBlock(new HorizontalRuleBlock());
				break;
			case "UL": case "OL":
				// TODO
				break;
			case "BLOCKQUOTE":
				AddBlock(new QuoteBlock { Blocks = TransformAll(node.ChildNodes) });
				break;
			case "TABLE":
				// TODO
				break;
			case not null when IsBlockTag(tagName):
				// Make sure the contents of this block are not displayed inline.
				FlushGenericBlock();
				ProcessAll(node.ChildNodes);
				FlushGenericBlock();
				break;
			default:
				// Wrap top-level inline content in a generic block.
				_currentGenericProcessor.Process(node);
				break;
		}
	}

	private void FlushGenericBlock()
	{
		if (_currentGenericProcessor.HasInlines)
		{
			_blocks.Add(new GenericBlock { Inlines = _currentGenericProcessor.GetResult() });
			_currentGenericProcessor.Reset();
		}
	}

	private void AddBlock(Block block)
	{
		FlushGenericBlock();
		_blocks.Add(block);
	}

	private readonly List<Block> _blocks = new();
	private readonly HtmlInlineProcessor _currentGenericProcessor = new();
}
