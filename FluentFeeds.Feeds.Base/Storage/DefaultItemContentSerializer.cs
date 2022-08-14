using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using FluentFeeds.Documents;
using FluentFeeds.Feeds.Base.Items.Content;
using FluentFeeds.Feeds.Base.Items.ContentLoaders;

namespace FluentFeeds.Feeds.Base.Storage;

/// <summary>
/// Default item content serializer implementation which stores the whole content as a JSON string.
/// </summary>
public sealed class DefaultItemContentSerializer : IItemContentSerializer
{
	private record SerializedContent(ItemContentType Type, RichText? ArticleBody);
	
	public async Task<string> StoreAsync(IItemContentLoader contentLoader)
	{
		var loaded = await contentLoader.LoadAsync();
		var content = new SerializedContent(loaded.Type, (loaded as ArticleItemContent)?.Body);
		var options = new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };
		return JsonSerializer.Serialize(content, options);
	}

	public Task<IItemContentLoader> LoadAsync(string serialized)
	{
		var options = new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };
		var content = JsonSerializer.Deserialize<SerializedContent>(serialized, options) ?? throw new JsonException();
		return Task.FromResult<IItemContentLoader>(new StaticItemContentLoader(
			content.Type switch
			{
				ItemContentType.Article => new ArticleItemContent(content.ArticleBody ?? new RichText()),
				_ => throw new IndexOutOfRangeException()
			}));
	}
}
