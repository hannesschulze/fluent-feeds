using System;
using System.Collections.Immutable;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentFeeds.Shared.RichText.Inlines;

namespace FluentFeeds.Shared.RichText.Json;

/// <summary>
/// JSON converter for inlines, supporting polymorphism.
/// </summary>
public sealed class InlineJsonConverter<TInline> : JsonConverter<TInline> where TInline : Inline
{
	private static Inline ReadBase(ref Utf8JsonReader reader, JsonSerializerOptions options)
	{
		if (reader.TokenType == JsonTokenType.String)
		{
			return new TextInline(reader.GetString() ?? String.Empty);
		}
		else if (reader.TokenType == JsonTokenType.StartObject)
		{
			InlineType? type = null;
			string? text = null;
			ImmutableArray<Inline>? inlines = null;
			Uri? target = null;
			Uri? source = null;
			string? alternateText = null;
			int? width = null;
			int? height = null;

			while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
			{
				if (reader.TokenType == JsonTokenType.PropertyName)
				{
					var propertyName = reader.GetString();
					reader.Read();
					switch (propertyName)
					{
						case "Type":
							type = JsonSerializer.Deserialize<InlineType>(ref reader, options);
							break;
						case "Text":
							text = reader.GetString();
							break;
						case "Inlines":
							inlines = JsonSerializer.Deserialize<ImmutableArray<Inline>>(ref reader, options);
							break;
						case "Target":
							target = JsonSerializer.Deserialize<Uri>(ref reader, options);
							break;
						case "Source":
							source = JsonSerializer.Deserialize<Uri>(ref reader, options);
							break;
						case "AlternateText":
							alternateText = reader.GetString();
							break;
						case "Width":
							width = reader.GetInt32();
							break;
						case "Height":
							height = reader.GetInt32();
							break;
					}
				}
			}

			return
				type switch
				{
					InlineType.Text => new TextInline { Text = text ?? String.Empty },
					InlineType.Image =>
						new ImageInline
						{
							Source = source,
							AlternateText = alternateText,
							Width = width ?? -1,
							Height = height ?? -1
						},
					InlineType.Bold => new BoldInline { Inlines = inlines ?? ImmutableArray<Inline>.Empty },
					InlineType.Italic => new ItalicInline { Inlines = inlines ?? ImmutableArray<Inline>.Empty },
					InlineType.Underline => new UnderlineInline { Inlines = inlines ?? ImmutableArray<Inline>.Empty },
					InlineType.Strikethrough =>
						new StrikethroughInline { Inlines = inlines ?? ImmutableArray<Inline>.Empty },
					InlineType.Code => new CodeInline { Inlines = inlines ?? ImmutableArray<Inline>.Empty },
					InlineType.Hyperlink =>
						new HyperlinkInline { Inlines = inlines ?? ImmutableArray<Inline>.Empty, Target = target },
					_ => throw new JsonException("Inline missing type")
				};
		}
		else
		{
			throw new JsonException();
		}
	}

	public override TInline Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var baseResult = ReadBase(ref reader, options);
		if (baseResult is not TInline result)
			throw new JsonException("Inline with invalid type");
		
		return result;
	}

	private sealed class Visitor : IInlineVisitor
	{
		public Visitor(Utf8JsonWriter writer, JsonSerializerOptions options)
		{
			_writer = writer;
			_options = options;
		}
		
		public void Visit(TextInline inline)
		{
			_writer.WriteStringValue(inline.Text);
		}

		public void Visit(ImageInline inline)
		{
			WriteStart(inline);
			
			if (inline.Source != null)
			{
				_writer.WritePropertyName("Source");
				JsonSerializer.Serialize(_writer, inline.Source, _options);
			}
			
			if (inline.AlternateText != null)
			{
				_writer.WriteString("AlternateText", inline.AlternateText);
			}
			
			if (inline.Width >= 0)
			{
				_writer.WriteNumber("Width", inline.Width);
			}
			
			if (inline.Height >= 0)
			{
				_writer.WriteNumber("Height", inline.Height);
			}
			
			WriteEnd();
		}

		public void Visit(BoldInline inline)
		{
			WriteSimpleSpan(inline);
		}

		public void Visit(ItalicInline inline)
		{
			WriteSimpleSpan(inline);
		}

		public void Visit(UnderlineInline inline)
		{
			WriteSimpleSpan(inline);
		}

		public void Visit(StrikethroughInline inline)
		{
			WriteSimpleSpan(inline);
		}

		public void Visit(CodeInline inline)
		{
			WriteSimpleSpan(inline);
		}

		public void Visit(HyperlinkInline inline)
		{
			WriteStart(inline);
			
			if (inline.Target != null)
			{
				_writer.WritePropertyName("Target");
				JsonSerializer.Serialize(_writer, inline.Target, _options);
			}
			
			_writer.WritePropertyName("Inlines");
			JsonSerializer.Serialize(_writer, inline.Inlines, _options);
			
			WriteEnd();
		}

		private void WriteSimpleSpan(SpanInline inline)
		{
			WriteStart(inline);
			_writer.WritePropertyName("Inlines");
			JsonSerializer.Serialize(_writer, inline.Inlines, _options);
			WriteEnd();
		}

		private void WriteStart(Inline inline)
		{
			_writer.WriteStartObject();
			_writer.WritePropertyName("Type");
			JsonSerializer.Serialize(_writer, inline.Type, _options);
		}

		private void WriteEnd()
		{
			_writer.WriteEndObject();
		}
		
		private readonly Utf8JsonWriter _writer;
		private readonly JsonSerializerOptions _options;
	}

	public override void Write(Utf8JsonWriter writer, TInline value, JsonSerializerOptions options) => 
		value.Accept(new Visitor(writer, options));
}
