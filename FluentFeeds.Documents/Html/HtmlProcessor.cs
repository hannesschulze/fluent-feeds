using System.Collections.Generic;
using AngleSharp.Dom;

namespace FluentFeeds.Documents.Html;

/// <summary>
/// Base class for processing AngleSharp HTML elements.
/// </summary>
internal abstract class HtmlProcessor
{
	public HtmlProcessor(HtmlParsingOptions options)
	{
		Options = options;
	}
	
	public HtmlParsingOptions Options { get; }
	
	public abstract void Process(INode node);

	public void ProcessAll(IEnumerable<INode> nodes)
	{
		foreach (var node in nodes)
		{
			Process(node);
		}
	}
}
