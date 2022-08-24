using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentFeeds.App.Shared.EventArgs;
using FluentFeeds.App.Shared.Models.Items;
using FluentFeeds.Feeds.Base.Items;

namespace FluentFeeds.App.Shared.Models.Storage;

/// <summary>
/// Default item storage implementation.
/// </summary>
public sealed class ItemStorage : IItemStorage
{
	public event EventHandler<ItemsDeletedEventArgs>? ItemsDeleted;
	
	public Task<IEnumerable<IItemView>> GetItemsAsync(Guid feedIdentifier)
	{
		throw new NotImplementedException();
	}

	public Task<IEnumerable<IItemView>> AddItemsAsync(IEnumerable<ItemDescriptor> items, Guid feedIdentifier)
	{
		throw new NotImplementedException();
	}

	public Task<IItemView> SetItemReadAsync(Guid identifier, bool isRead)
	{
		throw new NotImplementedException();
	}

	public Task DeleteItemsAsync(IReadOnlyCollection<Guid> identifiers)
	{
		throw new NotImplementedException();
	}
}
