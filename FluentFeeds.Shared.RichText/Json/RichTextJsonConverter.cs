using System;
using System.Collections.Immutable;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentFeeds.Shared.RichText.Blocks;

namespace FluentFeeds.Shared.RichText.Json;

/// <summary>
/// JSON converter for <see cref="RichText"/> objects.
/// </summary>
public sealed class RichTextJsonConverter : JsonConverter<RichText>
{
	public override RichText Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		ImmutableArray<Block>? blocks = null;
		
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
					}
				}
			}
		}
		else
		{
			// Compact format
			blocks = JsonSerializer.Deserialize<ImmutableArray<Block>>(ref reader, options);			
		}

		return new RichText { Blocks = blocks ?? ImmutableArray<Block>.Empty };
	}

	public override void Write(Utf8JsonWriter writer, RichText value, JsonSerializerOptions options)
	{
		JsonSerializer.Serialize(writer, value.Blocks, options);
	}
}
