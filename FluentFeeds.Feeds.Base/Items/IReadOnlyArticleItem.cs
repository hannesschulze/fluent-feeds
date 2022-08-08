using FluentFeeds.Documents;

namespace FluentFeeds.Feeds.Base.Items;

public interface IReadOnlyArticleItem : IReadOnlyItem
{
	/// <summary>
	/// Content of the article.
	/// </summary>
	public RichText Content { get; }
}
