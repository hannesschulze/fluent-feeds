using System;
using FluentFeeds.Documents;
using FluentFeeds.Feeds.Base.Items;
using FluentFeeds.Feeds.Base.Items.Content;
using FluentFeeds.Feeds.Base.Storage;

namespace FluentFeeds.Feeds.Base.Tests;

public static class TestHelpers
{
	public static IReadOnlyStoredItem CreateItem(Guid identifier, IItemStorage storage) =>
		new StoredItem(
			identifier, storage, new Uri("https://www.example.com"), null, DateTimeOffset.Now, DateTimeOffset.Now,
			"title", "author", "summary", new StaticItemContentLoader(new ArticleItemContent(new RichText())), false);
}
