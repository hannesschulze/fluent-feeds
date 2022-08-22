using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using FluentFeeds.Documents;
using FluentFeeds.Feeds.Base.Factories;
using FluentFeeds.Feeds.Base.Items.Content;
using FluentFeeds.Feeds.Base.Items.ContentLoaders;
using FluentFeeds.Feeds.Base.Nodes;
using FluentFeeds.Feeds.Base.Storage;

namespace FluentFeeds.Feeds.Base;

/// <summary>
/// A pluggable class which can create feeds and has its own area in the sidebar.
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
	/// Create the initial set of feed nodes presented when the user adds this provider and there is no saved tree.
	/// </summary>
	public abstract IReadOnlyFeedNode CreateInitialTree(IFeedStorage feedStorage);
	
	/// <summary>
	/// Load a serialized feed as returned by <see cref="StoreFeedAsync"/>.
	/// </summary>
	public abstract Task<Feed> LoadFeedAsync(IFeedStorage feedStorage, string serialized);
	
	/// <summary>
	/// Serialize a feed so it can be loaded using <see cref="LoadFeedAsync"/>.
	/// </summary>
	public abstract Task<string> StoreFeedAsync(Feed feed);

	/// <summary>
	/// Serialize a item content into a string.
	/// </summary>
	/// <remarks>
	/// The default implementation loads the content and serializes it as JSON.
	/// </remarks>
	public virtual async Task<string> StoreContentAsync(IItemContentLoader contentLoader)
	{
		var loaded = await contentLoader.LoadAsync();
		var content = new SerializedContent(loaded.Type, (loaded as ArticleItemContent)?.Body);
		var options = new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };
		return JsonSerializer.Serialize(content, options);
	}

	/// <summary>
	/// Deserialize a content loader from a string.
	/// </summary>
	public virtual Task<IItemContentLoader> LoadContentAsync(string serialized)
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
