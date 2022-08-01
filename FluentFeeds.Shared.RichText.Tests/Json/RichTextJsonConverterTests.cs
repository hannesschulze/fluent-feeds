using System.Text.Json;
using FluentFeeds.Shared.RichText.Blocks;
using FluentFeeds.Shared.RichText.Inlines;
using Xunit;

namespace FluentFeeds.Shared.RichText.Tests.Json;

public class RichTextJsonConverterTests
{
	[Fact]
	public void ConvertSpecial_NullValue()
	{
		Assert.Null(JsonSerializer.Deserialize<RichText>("null"));
	}

	[Fact]
	public void ConvertSpecial_InvalidToken()
	{
		Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<RichText>("true"));
	}

	[Fact]
	public void Convert_CompactMode()
	{
		var original = new RichText(
			new GenericBlock(new TextInline("foo")),
			new ParagraphBlock(new TextInline("bar")));
		var json = JsonSerializer.Serialize(original);
		Assert.Equal(original, JsonSerializer.Deserialize<RichText>(json));
	}

	[Fact]
	public void Convert_NonCompactMode()
	{
		const string json = "{\"Blocks\": [{\"Type\": 0}, {\"Type\": 1}]}";
		var expected = new RichText(new GenericBlock(), new ParagraphBlock());
		Assert.Equal(expected, JsonSerializer.Deserialize<RichText>(json));
	}
}
