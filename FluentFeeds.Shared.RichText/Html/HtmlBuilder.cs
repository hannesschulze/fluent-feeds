using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FluentFeeds.Shared.RichText.Html;

/// <summary>
/// Helper class for building a HTML string.
/// </summary>
internal sealed class HtmlBuilder
{
	public HtmlBuilder(HtmlWritingOptions options)
	{
		_options = options;
		_scopes.Add(new Scope("", Inline: false, HasElements: false));
	}
	
	public string GetResult() => _builder.ToString();

	/// <summary>
	/// Temporarily disable all indentation.
	/// </summary>
	public bool DisableIndentation { get; set; }

	/// <summary>
	/// Escape and append plain text to the current scope.
	/// </summary>
	public HtmlBuilder AppendText(string text, bool transformLineBreaks = true)
	{
		PrepareNewElementInScope();
		AppendEscaped(text, transformLineBreaks, escapeQuotes: false, inline: _scopes.Last().Inline);
		return this;
	}

	/// <summary>
	/// Append an empty tag to the current scope.
	/// </summary>
	public HtmlBuilder AppendEmptyTag(string tag, IReadOnlyDictionary<string, string>? attributes = null)
	{
		PrepareNewElementInScope();
		_builder.Append('<').Append(tag);
		AppendAttributes(attributes);
		_builder.Append(_options.CloseEmptyTags ? "/>" : ">");
		return this;
	}

	/// <summary>
	/// Open a tag in the current scope, starting a new scope.
	/// </summary>
	/// <param name="tag">The tag name.</param>
	/// <param name="inline">Set to true if the tag should be printed to a single line.</param>
	/// <param name="attributes">HTML attributes (values are escaped).</param>
	public HtmlBuilder AppendTagOpen(string tag, bool inline, IReadOnlyDictionary<string, string>? attributes = null)
	{
		PrepareNewElementInScope();
		_builder.Append('<').Append(tag);
		AppendAttributes(attributes);
		_builder.Append('>');
		_scopes.Add(new Scope(tag, inline, false));
		if (!inline)
			_indentationLevel++;
		return this;
	}

	/// <summary>
	/// Close the current scope.
	/// </summary>
	public HtmlBuilder AppendTagClose()
	{
		if (_scopes.Count <= 1)
			// Don't remove the root scope.
			return this;

		var scope = _scopes.Last();
		_scopes.RemoveAt(_scopes.Count - 1);
		if (!scope.Inline)
		{
			_indentationLevel--;
			AppendLineBreak();
		}
		_builder.Append("</").Append(scope.Tag).Append('>');
		return this;
	}

	/// <summary>
	/// Add line breaks in preparation of a new HTML element.
	/// </summary>
	private void PrepareNewElementInScope()
	{
		var scope = _scopes.Last();
		if (!scope.Inline)
		{
			if (scope.HasElements)
			{
				// Add additional line breaks for separating tags.
				_builder.Append('\n', Math.Max(0, _options.EmptyLinesBetweenTags));
				AppendLineBreak();
			}
			else if (_scopes.Count > 1)
			{
				// Always append at least one line break (unless this is the root tag) because opening a tag does not
				// insert a line break.
				AppendLineBreak();
			}
		}
		_scopes[^1] = scope with {HasElements = true};
	}

	/// <summary>
	/// Append a line break and indentation.
	/// </summary>
	private void AppendLineBreak()
	{
		_builder.Append('\n');
		if (DisableIndentation)
			return;
		
		var count = _options.SpacesForIndentation;
		var character = ' ';
		if (count < 0)
		{
			count = 1;
			character = '\t';
		}

		_builder.Append(character, _indentationLevel * count);
	}

	/// <summary>
	/// Append a set of HTML attributes. A space is prepended and the attribute values are escaped.
	/// </summary>
	private void AppendAttributes(IReadOnlyDictionary<string, string>? attributes)
	{
		if (attributes == null || attributes.Count == 0)
			return;

		foreach (var attribute in attributes)
		{
			_builder.Append(' ').Append(attribute.Key).Append("=\"");
			AppendEscaped(attribute.Value, transformLineBreaks: false, escapeQuotes: true, inline: true);
			_builder.Append('\"');
		}
	}

	/// <summary>
	/// Escape a string.
	/// </summary>
	/// <param name="str">The string to escape.</param>
	/// <param name="transformLineBreaks">Set to true if newlines should be transformed to HTML line breaks.</param>
	/// <param name="escapeQuotes">Set to true if " and ' should be escaped.</param>
	/// <param name="inline">Set to true to avoid line breaks.</param>
	private void AppendEscaped(string str, bool transformLineBreaks, bool escapeQuotes, bool inline)
	{
		_builder.EnsureCapacity(_builder.Length + str.Length);

		foreach (var c in str)
		{
			switch (c)
			{
				case '&':
					_builder.Append("&amp;");
					break;
				case '<':
					_builder.Append("&lt;");
					break;
				case '>':
					_builder.Append("&gt;");
					break;
				case '\r':
					break;
				case '\n':
					if (transformLineBreaks)
						_builder.Append(_options.CloseEmptyTags ? "<br/>" : "<br>");
					else if (inline)
						_builder.Append("&#10;");
					
					if (!inline)
						AppendLineBreak();
					break;
				case '"' when escapeQuotes: 
					_builder.Append("&quot;");
					break;
				case '\'' when escapeQuotes:
					_builder.Append("&apos;");
					break;
				default: 
					_builder.Append(c); 
					break;
			}
		}
	}

	private readonly record struct Scope(string Tag, bool Inline, bool HasElements);

	private readonly HtmlWritingOptions _options;
	private readonly List<Scope> _scopes = new();
	private readonly StringBuilder _builder = new();
	private int _indentationLevel = 0;
}
