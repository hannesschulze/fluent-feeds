using System;
using System.Text.Json;
using FluentFeeds.Shared.RichText.Inlines;
using Xunit;

namespace FluentFeeds.Shared.RichText.Tests.Json;

public class InlineJsonConverterTests
{
	private static TInline EncodeAndDecode<TInline>(TInline inline) where TInline : Inline
	{
		var json = JsonSerializer.Serialize(inline);
		var result = JsonSerializer.Deserialize<TInline>(json);
		Assert.NotNull(result);
		return result!;
	}

	private static Inline EncodeAndDecodeBase(Inline inline)
	{
		var json = JsonSerializer.Serialize(inline);
		var result = JsonSerializer.Deserialize<Inline>(json);
		Assert.NotNull(result);
		return result!;
	}

	[Fact]
	public void ConvertSpecial_MissingType()
	{
		const string json = "{\"Text\":\"foo\"}";
		Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<Inline>(json));
		Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<TextInline>(json));
	}

	[Fact]
	public void ConvertSpecial_MismatchingType()
	{
		var json = JsonSerializer.Serialize(new TextInline("foo"));
		Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<BoldInline>(json));
	}

	[Fact]
	public void ConvertSpecial_NullValue()
	{
		Assert.Null(JsonSerializer.Deserialize<Inline>("null"));
		Assert.Null(JsonSerializer.Deserialize<TextInline>("null"));
	}

	[Fact]
	public void ConvertSpecial_InvalidType()
	{
		Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<Inline>("[]"));
		Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<TextInline>("[]"));
	}
	
	[Fact]
	public void ConvertText()
	{
		var original = new TextInline("foo");
		var decoded = EncodeAndDecode(original);
		var decodedBase = EncodeAndDecodeBase(original);
		Assert.Equal(original, decoded);
		Assert.Equal(original, decodedBase);
	}
	
	[Fact]
	public void ConvertImage_NoProperties()
	{
		var original = new ImageInline();
		var decoded = EncodeAndDecode(original);
		var decodedBase = EncodeAndDecodeBase(original);
		Assert.Equal(original, decoded);
		Assert.Equal(original, decodedBase);
	}
	
	[Fact]
	public void ConvertImage_AllProperties()
	{
		var original =
			new ImageInline(new Uri("https://www.example.com/image.png"))
			{
				AlternateText = "foo",
				Width = 50,
				Height = 30
			};
		var decoded = EncodeAndDecode(original);
		var decodedBase = EncodeAndDecodeBase(original);
		Assert.Equal(original, decoded);
		Assert.Equal(original, decodedBase);
	}
	
	[Fact]
	public void ConvertBold()
	{
		var original = new BoldInline(new TextInline("foo"), new TextInline("bar"));
		var decoded = EncodeAndDecode(original);
		var decodedBase = EncodeAndDecodeBase(original);
		Assert.Equal(original, decoded);
		Assert.Equal(original, decodedBase);
	}
	
	[Fact]
	public void ConvertItalic()
	{
		var original = new ItalicInline(new TextInline("foo"), new TextInline("bar"));
		var decoded = EncodeAndDecode(original);
		var decodedBase = EncodeAndDecodeBase(original);
		Assert.Equal(original, decoded);
		Assert.Equal(original, decodedBase);
	}
	
	[Fact]
	public void ConvertUnderline()
	{
		var original = new UnderlineInline(new TextInline("foo"), new TextInline("bar"));
		var decoded = EncodeAndDecode(original);
		var decodedBase = EncodeAndDecodeBase(original);
		Assert.Equal(original, decoded);
		Assert.Equal(original, decodedBase);
	}
	
	[Fact]
	public void ConvertStrikethrough()
	{
		var original = new StrikethroughInline(new TextInline("foo"), new TextInline("bar"));
		var decoded = EncodeAndDecode(original);
		var decodedBase = EncodeAndDecodeBase(original);
		Assert.Equal(original, decoded);
		Assert.Equal(original, decodedBase);
	}
	
	[Fact]
	public void ConvertCode()
	{
		var original = new CodeInline(new TextInline("foo"), new TextInline("bar"));
		var decoded = EncodeAndDecode(original);
		var decodedBase = EncodeAndDecodeBase(original);
		Assert.Equal(original, decoded);
		Assert.Equal(original, decodedBase);
	}
	
	[Fact]
	public void ConvertHyperlink_NoTarget()
	{
		var original = new HyperlinkInline(new TextInline("foo"), new TextInline("bar"));
		var decoded = EncodeAndDecode(original);
		var decodedBase = EncodeAndDecodeBase(original);
		Assert.Equal(original, decoded);
		Assert.Equal(original, decodedBase);
	}
	
	[Fact]
	public void ConvertHyperlink_WithTarget()
	{
		var original =
			new HyperlinkInline(new TextInline("foo"), new TextInline("bar"))
			{
				Target = new Uri("https://www.example.com")
			};
		var decoded = EncodeAndDecode(original);
		var decodedBase = EncodeAndDecodeBase(original);
		Assert.Equal(original, decoded);
		Assert.Equal(original, decodedBase);
	}
}
