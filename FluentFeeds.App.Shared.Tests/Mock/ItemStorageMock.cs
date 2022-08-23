using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentFeeds.Feeds.Base.EventArgs;
using FluentFeeds.Feeds.Base.Items;
using FluentFeeds.Feeds.Base.Storage;

namespace FluentFeeds.App.Shared.Tests.Mock;

public sealed class ItemStorageMock : IItemStorage
{
	public event EventHandler<ItemsDeletedEventArgs>? ItemsDeleted;
	public Task<IEnumerable<IReadOnlyStoredItem>> GetItemsAsync() => Task.FromResult(GetItems());

	public Task<IEnumerable<IReadOnlyStoredItem>> AddItemsAsync(IEnumerable<IReadOnlyItem> items) =>
		Task.FromResult(AddItems(items));

	public Task<IReadOnlyStoredItem> SetItemReadAsync(Guid identifier, bool isRead) =>
		Task.FromResult(SetItemRead(identifier, isRead));

	public Task DeleteItemsAsync(IReadOnlyCollection<Guid> identifiers)
	{
		DeleteItems(identifiers);
		return Task.CompletedTask;
	}

	public IEnumerable<IReadOnlyStoredItem> GetItems()
	{
		return _items.Values;
	}

	public IEnumerable<IReadOnlyStoredItem> AddItems(IEnumerable<IReadOnlyItem> items)
	{
		foreach (var item in items)
		{
			var stored = new StoredItem(item, Guid.NewGuid(), this, false);
			_items.Add(stored.Identifier, stored);
			yield return stored;
		}
	}

	public IReadOnlyStoredItem SetItemRead(Guid identifier, bool isRead)
	{
		var item = _items[identifier];
		item.IsRead = isRead;
		return item;
	}

	public void DeleteItems(IReadOnlyCollection<Guid> identifiers)
	{
		var items = new List<IReadOnlyStoredItem> { Capacity = identifiers.Count };
		foreach (var identifier in identifiers)
		{
			if (_items.Remove(identifier, out var item))
			{
				items.Add(item);
			}
		}
		ItemsDeleted?.Invoke(this, new ItemsDeletedEventArgs(items));
	}

	private readonly Dictionary<Guid, StoredItem> _items = new();
}
