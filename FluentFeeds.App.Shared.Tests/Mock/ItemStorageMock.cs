using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentFeeds.App.Shared.EventArgs;
using FluentFeeds.App.Shared.Models.Items;
using FluentFeeds.App.Shared.Models.Storage;
using FluentFeeds.Feeds.Base.Items;

namespace FluentFeeds.App.Shared.Tests.Mock;

public sealed class ItemStorageMock : IItemStorage
{
	public ItemStorageMock(Guid? identifier = null)
	{
		Identifier = identifier ?? Guid.Empty;
	}
	
	public Guid Identifier { get; }
	
	public event EventHandler<ItemsDeletedEventArgs>? ItemsDeleted;
	
	public Task<IEnumerable<IItemView>> GetItemsAsync(Guid feedIdentifier) =>
		Task.FromResult(GetItems(feedIdentifier));

	public Task<IEnumerable<IItemView>> AddItemsAsync(IEnumerable<ItemDescriptor> items, Guid feedIdentifier) =>
		Task.FromResult(AddItems(items, feedIdentifier));

	public Task<IItemView> SetItemReadAsync(Guid identifier, bool isRead) =>
		Task.FromResult(SetItemRead(identifier, isRead));

	public Task DeleteItemsAsync(IReadOnlyCollection<Guid> identifiers)
	{
		DeleteItems(identifiers);
		return Task.CompletedTask;
	}

	public IEnumerable<IItemView> GetItems(Guid feedIdentifier)
	{
		return _feedItems.TryGetValue(feedIdentifier, out var items) ? items : Enumerable.Empty<IItemView>();
	}

	public IEnumerable<IItemView> AddItems(IEnumerable<ItemDescriptor> descriptors, Guid feedIdentifier)
	{
		if (!_feedItems.TryGetValue(feedIdentifier, out var items))
		{
			items = new HashSet<Item>();
			_feedItems.Add(feedIdentifier, items);
		}
		foreach (var descriptor in descriptors)
		{
			var item = new Item(
				identifier: Guid.NewGuid(),
				storage: this,
				userIdentifier: descriptor.Identifier,
				title: descriptor.Title,
				author: descriptor.Author,
				summary: descriptor.Summary,
				publishedTimestamp: descriptor.PublishedTimestamp,
				modifiedTimestamp: descriptor.ModifiedTimestamp,
				url: descriptor.Url,
				contentUrl: descriptor.ContentUrl,
				isRead: false,
				contentLoader: descriptor.ContentLoader);
			_items.Add(item.Identifier, item);
			items.Add(item);
			yield return item;
		}
	}

	public IItemView SetItemRead(Guid identifier, bool isRead)
	{
		var item = _items[identifier];
		item.IsRead = isRead;
		return item;
	}

	public void DeleteItems(IReadOnlyCollection<Guid> identifiers)
	{
		var items = new List<Item> { Capacity = identifiers.Count };
		
		foreach (var identifier in identifiers)
		{
			if (_items.Remove(identifier, out var item))
			{
				items.Add(item);
			}
		}

		foreach (var feedItems in _feedItems.Values)
		{
			foreach (var item in items)
			{
				feedItems.Remove(item);
			}
		}
		
		ItemsDeleted?.Invoke(this, new ItemsDeletedEventArgs(items));
	}

	private readonly Dictionary<Guid, Item> _items = new();
	private readonly Dictionary<Guid, HashSet<Item>> _feedItems = new();
}
