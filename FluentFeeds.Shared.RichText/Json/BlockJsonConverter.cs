using System;
using System.Collections.Immutable;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentFeeds.Shared.RichText.Blocks;
using FluentFeeds.Shared.RichText.Blocks.Heading;
using FluentFeeds.Shared.RichText.Blocks.List;
using FluentFeeds.Shared.RichText.Blocks.Table;
using FluentFeeds.Shared.RichText.Inlines;

namespace FluentFeeds.Shared.RichText.Json;

/// <summary>
/// JSON converter for blocks, supporting polymorphism.
/// </summary>
public sealed class BlockJsonConverter<TBlock> : JsonConverter<TBlock> where TBlock : Block
{
	private static Block ReadBase(ref Utf8JsonReader reader, JsonSerializerOptions options)
	{
		if (reader.TokenType != JsonTokenType.StartObject)
			throw new JsonException();

		BlockType? type = null;
		ImmutableArray<Inline>? inlines = null;
		HeadingLevel? level = null;
		string? code = null;
		ImmutableArray<ListItem>? items = null;
		ImmutableArray<Block>? blocks = null;
		ImmutableArray<TableRow>? rows = null;

		while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
		{
			if (reader.TokenType == JsonTokenType.PropertyName)
			{
				var propertyName = reader.GetString();
				reader.Read();
				switch (propertyName)
				{
					case "Type":
						type = JsonSerializer.Deserialize<BlockType>(ref reader, options);
						break;
					case "Inlines":
						inlines = JsonSerializer.Deserialize<ImmutableArray<Inline>>(ref reader, options);
						break;
					case "Level":
						level = JsonSerializer.Deserialize<HeadingLevel>(ref reader, options);
						break;
					case "Code":
						code = reader.GetString();
						break;
					case "Items":
						items = JsonSerializer.Deserialize<ImmutableArray<ListItem>>(ref reader, options);
						break;
					case "Blocks":
						blocks = JsonSerializer.Deserialize<ImmutableArray<Block>>(ref reader, options);
						break;
					case "Rows":
						rows = JsonSerializer.Deserialize<ImmutableArray<TableRow>>(ref reader, options);
						break;
				}
			}
		}

		return
			type switch
			{
				BlockType.Generic => new GenericBlock { Inlines = inlines ?? ImmutableArray<Inline>.Empty },
				BlockType.Paragraph => new ParagraphBlock { Inlines = inlines ?? ImmutableArray<Inline>.Empty },
				BlockType.Heading =>
					new HeadingBlock
					{
						Inlines = inlines ?? ImmutableArray<Inline>.Empty,
						Level = level ?? HeadingLevel.Level1
					},
				BlockType.Code => new CodeBlock { Code = code ?? String.Empty },
				BlockType.HorizontalRule => new HorizontalRuleBlock(),
				BlockType.List => new ListBlock { Items = items ?? ImmutableArray<ListItem>.Empty },
				BlockType.Quote => new QuoteBlock { Blocks = blocks ?? ImmutableArray<Block>.Empty },
				BlockType.Table => new TableBlock { Rows = rows ?? ImmutableArray<TableRow>.Empty },
				_ => throw new JsonException("Block missing type")
			};
	}

	public override TBlock Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var baseResult = ReadBase(ref reader, options);
		if (baseResult is not TBlock result)
			throw new JsonException("Block with invalid type");
		
		return result;
	}

	private sealed class Visitor : IBlockVisitor
	{
		public Visitor(Utf8JsonWriter writer, JsonSerializerOptions options)
		{
			_writer = writer;
			_options = options;
		}
		
		public void Visit(GenericBlock block)
		{
			_writer.WritePropertyName("Inlines");
			JsonSerializer.Serialize(_writer, block.Inlines, _options);
		}

		public void Visit(ParagraphBlock block)
		{
			_writer.WritePropertyName("Inlines");
			JsonSerializer.Serialize(_writer, block.Inlines, _options);
		}

		public void Visit(HeadingBlock block)
		{
			_writer.WritePropertyName("Inlines");
			JsonSerializer.Serialize(_writer, block.Inlines, _options);
			_writer.WritePropertyName("Level");
			JsonSerializer.Serialize(_writer, block.Level, _options);
		}

		public void Visit(CodeBlock block)
		{
			_writer.WriteString("Code", block.Code);
		}

		public void Visit(HorizontalRuleBlock block)
		{
		}

		public void Visit(ListBlock block)
		{
			_writer.WritePropertyName("Items");
			JsonSerializer.Serialize(_writer, block.Items, _options);
		}

		public void Visit(QuoteBlock block)
		{
			_writer.WritePropertyName("Blocks");
			JsonSerializer.Serialize(_writer, block.Blocks, _options);
		}

		public void Visit(TableBlock block)
		{
			_writer.WritePropertyName("Rows");
			JsonSerializer.Serialize(_writer, block.Rows, _options);
		}
		
		private readonly Utf8JsonWriter _writer;
		private readonly JsonSerializerOptions _options;
	}

	public override void Write(Utf8JsonWriter writer, TBlock value, JsonSerializerOptions options)
	{
		writer.WriteStartObject();
		writer.WritePropertyName("Type");
		JsonSerializer.Serialize(writer, value.Type, options);
		value.Accept(new Visitor(writer, options));
		writer.WriteEndObject();
	}
}
