using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentFeeds.Feeds.Base.Items;
using FluentFeeds.Feeds.Base.Storage;

namespace FluentFeeds.Feeds.Base.Tests.Mock;

public sealed class ItemStorageMock : IItemStorage
{
	public Task<IEnumerable<IReadOnlyStoredItem>> GetItemsAsync() => Task.FromResult(GetItems());

	public Task<IEnumerable<IReadOnlyStoredItem>> AddItemsAsync(IEnumerable<IReadOnlyItem> items) =>
		Task.FromResult(AddItems(items));

	public IEnumerable<IReadOnlyStoredItem> GetItems()
	{
		return _items;
	}

	public IEnumerable<IReadOnlyStoredItem> AddItems(IEnumerable<IReadOnlyItem> items)
	{
		foreach (var item in items)
		{
			var stored = new StoredItem(item, Guid.NewGuid(), false);
			_items.Add(stored);
			yield return stored;
		}
	}

	private readonly List<IReadOnlyStoredItem> _items = new();
}
