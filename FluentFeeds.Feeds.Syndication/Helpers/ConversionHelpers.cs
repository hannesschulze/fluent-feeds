using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Threading.Tasks;
using FluentFeeds.Common;
using FluentFeeds.Documents;
using FluentFeeds.Documents.Html;
using FluentFeeds.Feeds.Base;
using FluentFeeds.Feeds.Base.Items;
using FluentFeeds.Feeds.Base.Items.Content;
using SysSyndicationFeed = System.ServiceModel.Syndication.SyndicationFeed;

namespace FluentFeeds.Feeds.Syndication.Helpers;

public static class ConversionHelpers
{
	/// <summary>
	/// Convert a list of persons into an author string.
	/// </summary>
	public static string? ConvertAuthor(IEnumerable<SyndicationPerson> authors)
	{
		var names = authors.Select(author => author.Name).Where(name => name != null).ToArray();
		return names.Length != 0 ? String.Join(", ", names) : null;
	}

	/// <summary>
	/// Convert an item's URL from its set of links.
	/// </summary>
	public static Uri? ConvertItemUrl(SyndicationItem item)
	{
		foreach (var link in item.Links)
		{
			if (link.RelationshipType == "alternate")
			{
				return link.Uri;
			}
		}

		return item.Links.FirstOrDefault()?.Uri;
	}
	
	/// <summary>
	/// Convert a syndication item's content if the item has the provided summary.
	/// </summary>
	public static async Task<RichText> ConvertItemContentAsync(
		SyndicationItem item, TextContent? summary, HtmlParsingOptions htmlOptions)
	{
		if (item.Content != null)
		{
			var content = await TextContent.LoadAsync(item.Content, htmlOptions).ConfigureAwait(false);
			return content.ToRichText();
		}

		var encodedContent =
			item.ElementExtensions.ReadElementExtensions<string>("encoded", "http://purl.org/rss/1.0/modules/content/");
		if (encodedContent != null && encodedContent.Count != 0)
		{
			var content = await TextContent.LoadAsync(encodedContent.Last(), htmlOptions).ConfigureAwait(false);
			return content.ToRichText();
		}

		return summary?.ToRichText() ?? new RichText();
	}

	/// <summary>
	/// Convert a syndication item into a generic <see cref="Item"/> object.
	/// </summary>
	public static async Task<IReadOnlyItem> ConvertItemAsync(SyndicationItem item, Uri feedUrl)
	{
		var htmlOptions = new HtmlParsingOptions { BaseUri = item.BaseUri ?? feedUrl };
		var title = 
			item.Title != null ? await TextContent.LoadAsync(item.Title, htmlOptions).ConfigureAwait(false) : null;
		var author = item.Authors != null ? ConvertAuthor(item.Authors) : null;
		var summary =
			item.Summary != null ? await TextContent.LoadAsync(item.Summary, htmlOptions).ConfigureAwait(false) : null;
		var content = await ConvertItemContentAsync(item, summary, htmlOptions).ConfigureAwait(false);
		var url = ConvertItemUrl(item);
		return new Item(
			url, contentUrl: null, publishedTimestamp: item.PublishDate, modifiedTimestamp: item.LastUpdatedTime,
			title?.ToPlainText() ?? String.Empty, author, summary?.ToPlainText() ?? content.ToPlainText(),
			new ArticleItemContent(content));
	}

	/// <summary>
	/// Convert the metadata of a syndication feed into a generic <see cref="FeedMetadata"/> object.
	/// </summary>
	public static async Task<FeedMetadata> ConvertFeedMetadataAsync(SysSyndicationFeed feed, Uri feedUrl)
	{
		var htmlOptions = new HtmlParsingOptions { BaseUri = feed.BaseUri ?? feedUrl };
		var title =
			feed.Title != null ? await TextContent.LoadAsync(feed.Title, htmlOptions).ConfigureAwait(false) : null;
		var authors = feed.Authors != null ? ConvertAuthor(feed.Authors) : null;
		var description = feed.Description != null
			? await TextContent.LoadAsync(feed.Description, htmlOptions).ConfigureAwait(false)
			: null;
		return new FeedMetadata(title?.ToPlainText(), authors, description?.ToPlainText(), Symbol.Web);
	}
}
