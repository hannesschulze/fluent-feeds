using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Html.Parser;
using FluentFeeds.Shared.RichText.Blocks;
using FluentFeeds.Shared.RichText.Helpers;
using FluentFeeds.Shared.RichText.Html;

namespace FluentFeeds.Shared.RichText;

/// <summary>
/// <param>A collection of blocks making up rich text.</param>
///
/// <param>Like all other rich text data models, this class is immutable.</param>
/// </summary>
public sealed class RichText : IEquatable<RichText>
{
	/// <summary>
	/// Create a rich text object by asynchronously parsing HTML input.
	/// </summary>
	public static async Task<RichText> ParseHtmlAsync(string html, HtmlParsingOptions? options = null)
	{
		options ??= new HtmlParsingOptions();
		var configuration = Configuration.Default
			.WithOnly(ctx => new HtmlParser(new HtmlParserOptions { IsStrictMode = options.IsStrict }, ctx));
		var document = await BrowsingContext.New(configuration)
			.OpenAsync(res => res.Content(html))
			.ConfigureAwait(false);
		return document.Body == null
			? new RichText()
			: new RichText { Blocks = HtmlBlockProcessor.TransformAll(document.Body.ChildNodes) };
	}

	/// <summary>
	/// Create a rich text object by parsing HTML input.
	/// </summary>
	public static RichText ParseHtml(string html, HtmlParsingOptions? options = null)
	{
		return ParseHtmlAsync(html, options).Result;
	}

	/// <summary>
	/// Try to create a rich text object by parsing HTML input.
	/// </summary>
	public static bool TryParseHtml(string html, out RichText result, HtmlParsingOptions? options = null)
	{
		try
		{
			result = ParseHtml(html, options);
			return true;
		}
		catch (Exception)
		{
			result = new RichText();
			return false;
		}
	}
	
	/// <summary>
	/// Create a new default-constructed rich text object.
	/// </summary>
	public RichText()
	{
		Blocks = ImmutableArray<Block>.Empty;
	}

	/// <summary>
	/// Create a new rich text object from a list of blocks.
	/// </summary>
	public RichText(params Block[] blocks)
	{
		Blocks = ImmutableArray.Create(blocks);
	}

	/// <summary>
	/// A list of blocks making up the rich text object.
	/// </summary>
	public ImmutableArray<Block> Blocks { get; init; }

	/// <summary>
	/// Format this rich text object as a HTML string.
	/// </summary>
	public string ToHtml(HtmlWritingOptions? options = null)
	{
		var writer = new HtmlWriter(options ?? new HtmlWritingOptions());
		foreach (var block in Blocks)
			block.Accept(writer);
		return writer.GetResult();
	}

	public bool Equals(RichText? other)
	{
		if (ReferenceEquals(this, other))
			return true;
		if (ReferenceEquals(null, other))
			return false;
		return Blocks.SequenceEqual(other.Blocks);
	}

	public override bool Equals(object? obj)
	{
		if (ReferenceEquals(this, obj))
			return true;
		return obj is RichText other && Equals(other);
	}

	public override int GetHashCode()
	{
		return Blocks.SequenceHashCode();
	}

	public override string ToString() => ToHtml();

	public static bool operator ==(RichText? lhs, RichText? rhs) => lhs?.Equals(rhs) ?? ReferenceEquals(rhs, null);
	public static bool operator !=(RichText? lhs, RichText? rhs) => !(lhs == rhs);
}
