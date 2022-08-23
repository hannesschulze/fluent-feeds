using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentFeeds.Feeds.Base.EventArgs;
using FluentFeeds.Feeds.Base.Items;
using FluentFeeds.Feeds.Base.Storage;

namespace FluentFeeds.Feeds.Syndication.Tests.Mock;

public sealed class ItemStorageMock : IItemStorage
{
	public ItemStorageMock(Guid identifier)
	{
		Identifier = identifier;
	}
	
	public Guid Identifier { get; }

	public event EventHandler<ItemsDeletedEventArgs>? ItemsDeleted;

	public Task<IEnumerable<IReadOnlyStoredItem>> GetItemsAsync() =>
		Task.FromResult(Enumerable.Empty<IReadOnlyStoredItem>());

	public Task<IEnumerable<IReadOnlyStoredItem>> AddItemsAsync(IEnumerable<IReadOnlyItem> items) =>
		Task.FromResult<IEnumerable<IReadOnlyStoredItem>>(
			items.Select(item => new StoredItem(item, Guid.NewGuid(), this, false)));
	
	public Task<IReadOnlyStoredItem> SetItemReadAsync(Guid identifier, bool isRead) =>
		throw new NotSupportedException();

	public Task DeleteItemsAsync(IReadOnlyCollection<Guid> identifiers) =>
		throw new NotSupportedException();
}
