using System.Collections.Generic;
using AngleSharp.Dom;

namespace FluentFeeds.Shared.RichText.Html;

/// <summary>
/// Base class for processing AngleSharp HTML elements.
/// </summary>
internal abstract class HtmlProcessor
{
	public abstract void Process(INode node);

	public void ProcessAll(IEnumerable<INode> nodes)
	{
		foreach (var node in nodes)
		{
			Process(node);
		}
	}
}
