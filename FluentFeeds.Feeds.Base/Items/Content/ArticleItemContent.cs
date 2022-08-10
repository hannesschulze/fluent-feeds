using FluentFeeds.Documents;

namespace FluentFeeds.Feeds.Base.Items.Content;

/// <summary>
/// <para>Content of an article item.</para>
///
/// <para>This object only stores a rich text object hosting the article's body.</para>
/// </summary>
public sealed class ArticleItemContent : ItemContent
{
	public ArticleItemContent(RichText body)
	{
		Body = body;
	}
	
	public override void Accept(IItemContentVisitor visitor) => visitor.Visit(this);

	public override ItemContentType Type => ItemContentType.Article;
	
	/// <summary>
	/// Body of the article.
	/// </summary>
	public RichText Body { get; }
}
