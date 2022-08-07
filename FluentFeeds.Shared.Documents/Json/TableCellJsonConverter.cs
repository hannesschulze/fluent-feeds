using System;
using System.Collections.Immutable;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentFeeds.Shared.Documents.Blocks;
using FluentFeeds.Shared.Documents.Blocks.Table;

namespace FluentFeeds.Shared.Documents.Json;

/// <summary>
/// JSON converter for <see cref="TableCell"/> objects.
/// </summary>
public sealed class TableCellJsonConverter : JsonConverter<TableCell>
{
	public override TableCell Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		ImmutableArray<Block>? blocks = null;
		int? columnSpan = null;
		int? rowSpan = null;
		bool? isHeader = null;
		
		if (reader.TokenType == JsonTokenType.StartObject)
		{
			while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
			{
				if (reader.TokenType == JsonTokenType.PropertyName)
				{
					var propertyName = reader.GetString();
					reader.Read();
					switch (propertyName)
					{
						case "Blocks":
							blocks = JsonSerializer.Deserialize<ImmutableArray<Block>>(ref reader, options);
							break;
						case "ColumnSpan":
							columnSpan = reader.GetInt32();
							break;
						case "RowSpan":
							rowSpan = reader.GetInt32();
							break;
						case "IsHeader":
							isHeader = reader.GetBoolean();
							break;
					}
				}
			}
		}
		else
		{
			// Compact format
			blocks = JsonSerializer.Deserialize<ImmutableArray<Block>>(ref reader, options);			
		}

		return 
			new TableCell
			{
				Blocks = blocks ?? ImmutableArray<Block>.Empty,
				ColumnSpan = columnSpan ?? 1,
				RowSpan = rowSpan ?? 1,
				IsHeader = isHeader ?? false
			};
	}

	public override void Write(Utf8JsonWriter writer, TableCell value, JsonSerializerOptions options)
	{
		if (value is { ColumnSpan: 1, RowSpan: 1, IsHeader: false })
		{
			// Compact format
			JsonSerializer.Serialize(writer, value.Blocks, options);
		}
		else
		{
			writer.WriteStartObject();
			writer.WritePropertyName("Blocks");
			JsonSerializer.Serialize(writer, value.Blocks, options);
			writer.WriteNumber("ColumnSpan", value.ColumnSpan);
			writer.WriteNumber("RowSpan", value.RowSpan);
			writer.WriteBoolean("IsHeader", value.IsHeader);
			writer.WriteEndObject();
		}
	}
}
