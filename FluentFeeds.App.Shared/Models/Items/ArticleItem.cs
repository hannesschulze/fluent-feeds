using System;
using FluentFeeds.Documents;

namespace FluentFeeds.App.Shared.Models.Items;

/// <summary>
/// <para>An item which represents an article.</para>
///
/// <para>The main content of an article is a rich text object hosting the article's body.</para>
/// </summary>
public sealed class ArticleItem : Item
{
	public ArticleItem(
		DateTimeOffset timestamp, string title, string author, Uri url, RichText content,
		string? summary = null, Uri? contentUrl = null, bool isRead = false) 
		: base(timestamp, title, author, url, summary ?? content.ToPlainText(), contentUrl, isRead)
	{
		Content = content;
	}

	public override void Accept(IItemVisitor visitor) => visitor.Visit(this);

	public override ItemType Type => ItemType.Article;

	/// <summary>
	/// Content of the article.
	/// </summary>
	public RichText Content { get; }
}
