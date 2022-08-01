using System.Text.Json;
using FluentFeeds.Shared.RichText.Blocks;
using FluentFeeds.Shared.RichText.Blocks.Heading;
using FluentFeeds.Shared.RichText.Blocks.List;
using FluentFeeds.Shared.RichText.Blocks.Table;
using FluentFeeds.Shared.RichText.Inlines;
using Xunit;

namespace FluentFeeds.Shared.RichText.Tests.Json;

public class BlockJsonConverterTests
{
	private static TBlock EncodeAndDecode<TBlock>(TBlock block) where TBlock : Block
	{
		var json = JsonSerializer.Serialize(block);
		var result = JsonSerializer.Deserialize<TBlock>(json);
		Assert.NotNull(result);
		return result!;
	}

	private static Block EncodeAndDecodeBase(Block block)
	{
		var json = JsonSerializer.Serialize(block);
		var result = JsonSerializer.Deserialize<Block>(json);
		Assert.NotNull(result);
		return result!;
	}

	[Fact]
	public void ConvertSpecial_MissingType()
	{
		const string json = "{\"Inlines\":[]}";
		Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<GenericBlock>(json));
		Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<Block>(json));
	}

	[Fact]
	public void ConvertSpecial_MismatchingType()
	{
		var json = JsonSerializer.Serialize(new GenericBlock());
		Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<ParagraphBlock>(json));
	}

	[Fact]
	public void ConvertSpecial_NullValue()
	{
		Assert.Null(JsonSerializer.Deserialize<GenericBlock>("null"));
		Assert.Null(JsonSerializer.Deserialize<Block>("null"));
	}

	[Fact]
	public void ConvertSpecial_InvalidToken()
	{
		Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<GenericBlock>("true"));
		Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<Block>("true"));
	}
	
	[Fact]
	public void ConvertGeneric_Compact()
	{
		var original = new GenericBlock(new TextInline("foo"), new TextInline("bar"));
		Assert.Equal(original, EncodeAndDecode(original));
		Assert.Equal(original, EncodeAndDecodeBase(original));
	}
	
	[Fact]
	public void ConvertGeneric_NonCompact()
	{
		const string json = "{\"Type\": 0, \"Inlines\": [\"foo\", \"bar\"]}";
		var expected = new GenericBlock(new TextInline("foo"), new TextInline("bar"));
		Assert.Equal(expected, JsonSerializer.Deserialize<GenericBlock>(json));
		Assert.Equal(expected, JsonSerializer.Deserialize<Block>(json));
	}
	
	[Fact]
	public void ConvertParagraph()
	{
		var original = new ParagraphBlock(new TextInline("foo"), new TextInline("bar"));
		Assert.Equal(original, EncodeAndDecode(original));
		Assert.Equal(original, EncodeAndDecodeBase(original));
	}
	
	[Fact]
	public void ConvertHeading()
	{
		var original = new HeadingBlock(new TextInline("foo"), new TextInline("bar")) { Level = HeadingLevel.Level3 };
		Assert.Equal(original, EncodeAndDecode(original));
		Assert.Equal(original, EncodeAndDecodeBase(original));
	}
	
	[Fact]
	public void ConvertCode()
	{
		var original = new CodeBlock("foo");
		Assert.Equal(original, EncodeAndDecode(original));
		Assert.Equal(original, EncodeAndDecodeBase(original));
	}
	
	[Fact]
	public void ConvertHorizontalRule()
	{
		var original = new HorizontalRuleBlock();
		Assert.Equal(original, EncodeAndDecode(original));
		Assert.Equal(original, EncodeAndDecodeBase(original));
	}
	
	[Fact]
	public void ConvertList_CompactItems()
	{
		var original = new ListBlock(
			new ListItem(
				new GenericBlock(new TextInline("foo")),
				new GenericBlock(new TextInline("bar"))),
			new ListItem(new TextInline("baz")));
		Assert.Equal(original, EncodeAndDecode(original));
		Assert.Equal(original, EncodeAndDecodeBase(original));
	}
	
	[Fact]
	public void ConvertList_NonCompactItems()
	{
		const string json = "{\"Type\": 5, \"Items\": [{\"Blocks\": [{\"Type\": 0}, {\"Type\": 1}]}]}";
		var expected = new ListBlock(new ListItem(new GenericBlock(), new ParagraphBlock()));
		Assert.Equal(expected, JsonSerializer.Deserialize<ListBlock>(json));
		Assert.Equal(expected, JsonSerializer.Deserialize<Block>(json));
	}
	
	[Fact]
	public void ConvertQuote()
	{
		var original = new QuoteBlock(
			new GenericBlock(new TextInline("foo")),
			new ParagraphBlock(new TextInline("bar")));
		Assert.Equal(original, EncodeAndDecode(original));
		Assert.Equal(original, EncodeAndDecodeBase(original));
	}
	
	[Fact]
	public void ConvertTable_CompactRows()
	{
		var original = new TableBlock(
			new TableRow(
				new TableCell(new TextInline("foo")) { RowSpan = 2, IsHeader = true },
				new TableCell(new TextInline("bar")) { ColumnSpan = 2, IsHeader = true }),
			new TableRow(
				new TableCell(
					new ParagraphBlock(new TextInline("a")),
					new GenericBlock(new TextInline("b")))));
		Assert.Equal(original, EncodeAndDecode(original));
		Assert.Equal(original, EncodeAndDecodeBase(original));
	}

	[Fact]
	public void ConvertTable_NonCompactRows()
	{
		const string json = "{\"Type\": 7, \"Rows\": [{\"Cells\": [[], {\"IsHeader\": true}]}]}";
		var expected = new TableBlock(new TableRow(new TableCell(), new TableCell { IsHeader = true }));
		Assert.Equal(expected, JsonSerializer.Deserialize<TableBlock>(json));
		Assert.Equal(expected, JsonSerializer.Deserialize<Block>(json));
	}
}
