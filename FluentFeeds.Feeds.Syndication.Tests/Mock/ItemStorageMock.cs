using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentFeeds.Feeds.Base.Items;
using FluentFeeds.Feeds.Base.Storage;

namespace FluentFeeds.Feeds.Syndication.Tests.Mock;

public sealed class ItemStorageMock : IItemStorage
{
	public Task<IEnumerable<IReadOnlyStoredItem>> GetItemsAsync(Guid collectionIdentifier) =>
		Task.FromResult(Enumerable.Empty<IReadOnlyStoredItem>());

	public Task<IEnumerable<IReadOnlyStoredItem>> AddItemsAsync(
		IEnumerable<IReadOnlyItem> items, Guid collectionIdentifier) =>
		Task.FromResult<IEnumerable<IReadOnlyStoredItem>>(items.Select(item => new StoredItem(
			Guid.NewGuid(), item.Url, item.ContentUrl, item.PublishedTimestamp, item.ModifiedTimestamp, item.Title,
			item.Author, item.Summary, item.Content, false)));
}
