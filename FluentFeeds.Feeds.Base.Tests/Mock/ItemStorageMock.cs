using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentFeeds.Feeds.Base.Items;
using FluentFeeds.Feeds.Base.Storage;

namespace FluentFeeds.Feeds.Base.Tests.Mock;

public sealed class ItemStorageMock : IItemStorage
{
	public Task<IEnumerable<IReadOnlyStoredItem>> GetItemsAsync(Guid collectionIdentifier) =>
		Task.FromResult(GetItems(collectionIdentifier));

	public Task<IEnumerable<IReadOnlyStoredItem>> AddItemsAsync(
		IEnumerable<IReadOnlyItem> items, Guid collectionIdentifier) =>
		Task.FromResult(AddItems(items, collectionIdentifier));

	public IEnumerable<IReadOnlyStoredItem> GetItems(Guid collectionIdentifier)
	{
		return GetCollection(collectionIdentifier);
	}

	public IEnumerable<IReadOnlyStoredItem> AddItems(IEnumerable<IReadOnlyItem> items, Guid collectionIdentifier)
	{
		var collection = GetCollection(collectionIdentifier);
		foreach (var item in items)
		{
			var stored = new StoredItem(item, Guid.NewGuid(), false);
			collection.Add(stored);
			yield return stored;
		}
	}

	private List<IReadOnlyStoredItem> GetCollection(Guid identifier)
	{
		if (_collections.TryGetValue(identifier, out var existing))
			return existing;

		var collection = new List<IReadOnlyStoredItem>();
		_collections.Add(identifier, collection);
		return collection;
	}

	private readonly Dictionary<Guid, List<IReadOnlyStoredItem>> _collections = new();
}
