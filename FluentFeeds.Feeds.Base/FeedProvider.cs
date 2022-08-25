using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using FluentFeeds.Documents;
using FluentFeeds.Feeds.Base.Factories;
using FluentFeeds.Feeds.Base.Feeds;
using FluentFeeds.Feeds.Base.Feeds.Content;
using FluentFeeds.Feeds.Base.Items.Content;

namespace FluentFeeds.Feeds.Base;

/// <summary>
/// A pluggable class which is responsible for the serialization of feed content loaders and item content loaders, as
/// well as creating new feeds.
/// </summary>
public abstract class FeedProvider
{
	private record SerializedContent(ItemContentType Type, RichText? ArticleBody);
	
	protected FeedProvider(FeedProviderMetadata metadata)
	{
		Metadata = metadata;
	}
	
	/// <summary>
	/// Metadata for this feed provider.
	/// </summary>
	public FeedProviderMetadata Metadata { get; }

	/// <summary>
	/// Factory for creating feeds from URLs.
	/// </summary>
	public IUrlFeedFactory? UrlFeedFactory { get; protected set; }
	
	/// <summary>
	/// Create a descriptor for the initial feed tree presented when the user adds this provider and there is no saved
	/// tree.
	/// </summary>
	public abstract GroupFeedDescriptor CreateInitialTree();
	
	/// <summary>
	/// Serialize a feed content loader so it can be loaded using <see cref="LoadFeedAsync"/>.
	/// </summary>
	public abstract Task<string> StoreFeedAsync(IFeedContentLoader contentLoader);
	
	/// <summary>
	/// Load a serialized feed content loader as returned by <see cref="StoreFeedAsync"/>.
	/// </summary>
	public abstract Task<IFeedContentLoader> LoadFeedAsync(string serialized);

	/// <summary>
	/// Serialize an item content loader into a string.
	/// </summary>
	/// <remarks>
	/// The default implementation loads the content and serializes it as JSON.
	/// </remarks>
	public virtual async Task<string> StoreItemContentAsync(IItemContentLoader contentLoader)
	{
		var loaded = await contentLoader.LoadAsync();
		var content = new SerializedContent(loaded.Type, (loaded as ArticleItemContent)?.Body);
		var options = new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };
		return JsonSerializer.Serialize(content, options);
	}

	/// <summary>
	/// Deserialize an item content loader from a string.
	/// </summary>
	public virtual Task<IItemContentLoader> LoadItemContentAsync(string serialized)
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
