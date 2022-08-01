using System;
using System.Text.Json.Serialization;
using FluentFeeds.Shared.RichText.Json;

namespace FluentFeeds.Shared.RichText.Inlines;

/// <summary>
/// Inline embedding an image.
/// </summary>
[JsonConverter(typeof(InlineJsonConverter<ImageInline>))]
public sealed class ImageInline : Inline
{
	/// <summary>
	/// Create a new default-constructed image inline.
	/// </summary>
	public ImageInline()
	{
	}

	/// <summary>
	/// Create a new image inline with the provided image source URI.
	/// </summary>
	public ImageInline(Uri? source)
	{
		Source = source;
	}
	
	/// <summary>
	/// The source URI of the image to load.
	/// </summary>
	public Uri? Source { get; init; }
	
	/// <summary>
	/// Alternate text describing the image if it is unavailable.
	/// </summary>
	public string? AlternateText { get; init; }

	/// <summary>
	/// The width at which the image should be displayed, or a negative number to scale the image's width to fit.
	/// </summary>
	public int Width { get; init; } = -1;
	
	/// <summary>
	/// The height at which the image should be displayed, or a negative number to scale the image's height to fit.
	/// </summary>
	public int Height { get; init; } = -1;
	
	public override InlineType Type => InlineType.Image;

	public override void Accept(IInlineVisitor visitor) => visitor.Visit(this);
	
	public override string ToString() =>
		$"ImageInline {{ Source = {Source?.ToString() ?? "[null]"}, AlternateText = {AlternateText ?? "[null]"}, " +
		$"Width = {Width}, Height = {Height} }}";
	
	public override bool Equals(Inline? other)
	{
		if (ReferenceEquals(this, other))
			return true;
		if (!base.Equals(other))
			return false;
		return other is ImageInline casted && Source == casted.Source;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(base.GetHashCode(), Source);
	}
}
