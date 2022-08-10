using System;
using FluentFeeds.Documents;
using FluentFeeds.Feeds.Base.Items;
using FluentFeeds.Feeds.Base.Items.Content;

namespace FluentFeeds.Feeds.Base.Tests;

public static class TestHelpers
{
	public static IReadOnlyStoredItem CreateItem(Guid identifier) =>
		new StoredItem(
			identifier, new Uri("https://www.example.com"), null, DateTimeOffset.Now, DateTimeOffset.Now, "title",
			"author", "summary", new ArticleItemContent(new RichText()), false);
}
