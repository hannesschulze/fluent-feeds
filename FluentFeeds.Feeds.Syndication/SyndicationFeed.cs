using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.ServiceModel.Syndication;
using System.Threading.Tasks;
using System.Xml;
using FluentFeeds.Common;
using FluentFeeds.Documents;
using FluentFeeds.Documents.Blocks;
using FluentFeeds.Documents.Inlines;
using FluentFeeds.Feeds.Base;
using FluentFeeds.Feeds.Base.Items;
using FluentFeeds.Feeds.Base.Items.Content;
using FluentFeeds.Feeds.Base.Storage;
using SysSyndicationFeed = System.ServiceModel.Syndication.SyndicationFeed;

namespace FluentFeeds.Feeds.Syndication;

public sealed class SyndicationFeed : CachedFeed
{
	public SyndicationFeed(IItemStorage storage, Guid collectionIdentifier, Uri url)
		: base(storage, collectionIdentifier)
	{
		Url = url;
	}

	public Uri Url { get; }

	private static async Task<string> ExtractPlainTextAsync(TextSyndicationContent content) =>
		content.Type switch
		{
			"html" or "xhtml" => (await RichText.ParseHtmlAsync(content.Text)).ToPlainText(),
			"text" or _ => content.Text
		};

	private static Task<RichText> ExtractRichTextAsync(SyndicationContent content) =>
		content switch
		{
			TextSyndicationContent textContent =>
				textContent.Type switch
				{
					"html" or "xhtml" => RichText.ParseHtmlAsync(textContent.Text),
					"text" or _ => Task.FromResult(new RichText(new GenericBlock(new TextInline(textContent.Text))))
				},
			UrlSyndicationContent urlContent =>
				Task.FromResult(new RichText(new GenericBlock(
					new HyperlinkInline(new TextInline(urlContent.Url.ToString())) { Target = urlContent.Url }))),
			_ => throw new NotSupportedException()
		};

	private static string ConvertAuthors(IEnumerable<SyndicationPerson> authors) =>
		String.Join(", ", authors.Select(author => author.Name));

	private static async Task<IReadOnlyItem> ConvertItemAsync(SyndicationItem item)
	{
		var title = await ExtractPlainTextAsync(item.Title);
		var authors = ConvertAuthors(item.Authors);
		var summary = await ExtractPlainTextAsync(item.Summary);
		var content = await ExtractRichTextAsync(item.Content);
		return new Item(
			url: item.BaseUri, contentUrl: null, publishedTimestamp: item.PublishDate,
			modifiedTimestamp: item.LastUpdatedTime, title, authors, summary, new ArticleItemContent(content));
	}
	
	private async Task<SysSyndicationFeed> FetchSyndicationFeedAsync()
	{
		await using var stream = await _httpClient.GetStreamAsync(Url);
		using var xmlReader = XmlReader.Create(stream);
		return SysSyndicationFeed.Load(xmlReader);
	}

	protected override async Task<IEnumerable<IReadOnlyItem>> DoFetchAsync()
	{
		var feed = await FetchSyndicationFeedAsync();
		
		// Update feed metadata
		var title = await ExtractPlainTextAsync(feed.Title);
		var authors = ConvertAuthors(feed.Authors);
		var description = await ExtractPlainTextAsync(feed.Description);
		Metadata = new FeedMetadata(title, authors, description, Symbol.Web);
		
		// Convert items in parallel
		return await Task.WhenAll(feed.Items.Select(ConvertItemAsync));
	}

	private readonly HttpClient _httpClient = new();
}
