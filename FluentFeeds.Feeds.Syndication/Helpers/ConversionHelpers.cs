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
using FluentFeeds.Feeds.Base.Items.ContentLoaders;
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
			var content = await TextContent.LoadAsync(item.Content, htmlOptions);
			return content.ToRichText();
		}

		var encodedContent =
			item.ElementExtensions.ReadElementExtensions<string>("encoded", "http://purl.org/rss/1.0/modules/content/");
		if (encodedContent != null && encodedContent.Count != 0)
		{
			var content = await TextContent.LoadAsync(encodedContent.Last(), htmlOptions);
			return content.ToRichText();
		}

		return summary?.ToRichText() ?? new RichText();
	}

	/// <summary>
	/// Convert a syndication item's content to a string.
	/// </summary>
	/// <param name="item"></param>
	/// <returns></returns>
	public static string? ConvertItemAuthor(SyndicationItem item)
	{
		var author = ConvertAuthor(item.Authors);
		if (author != null)
		{
			return author;
		}

		var creator = item.ElementExtensions.ReadElementExtensions<string>("creator", "http://purl.org/dc/elements/1.1/");
		if (creator != null && creator.Count != 0)
		{
			return creator.Last();
		}

		return null;
	}

	/// <summary>
	/// Convert a syndication item into a generic <see cref="Item"/> object.
	/// </summary>
	public static async Task<IReadOnlyItem> ConvertItemAsync(SyndicationItem item, Uri feedUrl)
	{
		var htmlOptions = new HtmlParsingOptions { BaseUri = item.BaseUri ?? feedUrl };
		var title = item.Title != null ? await TextContent.LoadAsync(item.Title, htmlOptions) : null;
		var author = ConvertItemAuthor(item);
		var summary = item.Summary != null ? await TextContent.LoadAsync(item.Summary, htmlOptions) : null;
		var content = await ConvertItemContentAsync(item, summary, htmlOptions);
		var url = ConvertItemUrl(item);
		return new Item(
			url, contentUrl: null, publishedTimestamp: item.PublishDate, modifiedTimestamp: item.LastUpdatedTime,
			title?.ToPlainText() ?? String.Empty, author, summary?.ToPlainText() ?? content.ToPlainText(),
			new StaticItemContentLoader(new ArticleItemContent(content)));
	}

	/// <summary>
	/// Convert the metadata of a syndication feed into a generic <see cref="FeedMetadata"/> object.
	/// </summary>
	public static async Task<FeedMetadata> ConvertFeedMetadataAsync(SysSyndicationFeed feed, Uri feedUrl)
	{
		var htmlOptions = new HtmlParsingOptions { BaseUri = feed.BaseUri ?? feedUrl };
		var title = feed.Title != null ? await TextContent.LoadAsync(feed.Title, htmlOptions) : null;
		var authors = feed.Authors != null ? ConvertAuthor(feed.Authors) : null;
		var description = feed.Description != null ? await TextContent.LoadAsync(feed.Description, htmlOptions) : null;
		return
			new FeedMetadata
			{
				Name = title?.ToPlainText(),
				Author = authors,
				Description = description?.ToPlainText(),
				Symbol = Symbol.Web
			};
	}
}
