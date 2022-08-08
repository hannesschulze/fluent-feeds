using System;
using FluentFeeds.Documents;

namespace FluentFeeds.Feeds.Base.Items;

/// <summary>
/// <para>An item which represents an article.</para>
///
/// <para>The main content of an article is a rich text object hosting the article's body.</para>
/// </summary>
public sealed class ArticleItem : Item, IReadOnlyArticleItem
{
	public ArticleItem(
		Guid identifier, Uri url, DateTimeOffset publishedTimestamp, DateTimeOffset modifiedTimestamp, string title, 
		string author, RichText content, string? summary = null, Uri? contentUrl = null, bool isRead = false) 
		: base(identifier, url, publishedTimestamp, modifiedTimestamp, title, author, summary, contentUrl, isRead)
	{
		_content = content;
	}

	public override void Accept(IItemVisitor visitor) => visitor.Visit(this);

	public override ItemType Type => ItemType.Article;

	public RichText Content
	{
		get => _content;
		set => SetProperty(ref _content, value);
	}

	private RichText _content;
}
