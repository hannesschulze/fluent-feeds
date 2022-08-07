using System;
using System.Collections.Immutable;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentFeeds.Shared.Documents.Blocks.Table;

namespace FluentFeeds.Shared.Documents.Json;

/// <summary>
/// JSON converter for <see cref="TableRow"/> objects.
/// </summary>
public sealed class TableRowJsonConverter : JsonConverter<TableRow>
{
	public override TableRow Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		ImmutableArray<TableCell>? cells = null;
		
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
						case "Cells":
							cells = JsonSerializer.Deserialize<ImmutableArray<TableCell>>(ref reader, options);
							break;
					}
				}
			}
		}
		else
		{
			// Compact format
			cells = JsonSerializer.Deserialize<ImmutableArray<TableCell>>(ref reader, options);			
		}

		return new TableRow { Cells = cells ?? ImmutableArray<TableCell>.Empty };
	}

	public override void Write(Utf8JsonWriter writer, TableRow value, JsonSerializerOptions options)
	{
		JsonSerializer.Serialize(writer, value.Cells, options);
	}
}
