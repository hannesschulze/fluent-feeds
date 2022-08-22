using System;
using FluentFeeds.Feeds.Base.Items.ContentLoaders;
using FluentFeeds.Feeds.Base.Storage;

namespace FluentFeeds.Feeds.Base.Items;

/// <summary>
/// <para>A mutable representation of a persistently stored item.</para>
///
/// <para>This adds a unique identifier and a "read" flag to the base item.</para>
/// </summary>
public class StoredItem : Item, IReadOnlyStoredItem
{
	public StoredItem(
		Guid identifier, IItemStorage storage, Uri? url, Uri? contentUrl, DateTimeOffset publishedTimestamp,
		DateTimeOffset modifiedTimestamp, string title, string? author, string? summary,
		IItemContentLoader contentLoader, bool isRead)
		: base(url, contentUrl, publishedTimestamp, modifiedTimestamp, title, author, summary, contentLoader)
	{
		Identifier = identifier;
		Storage = storage;
		_isRead = isRead;
	}
	
	/// <summary>
	/// Create a stored item from a base item.
	/// </summary>
	public StoredItem(IReadOnlyItem item, Guid identifier, IItemStorage storage, bool isRead) : base(item)
	{
		Identifier = identifier;
		Storage = storage;
		_isRead = isRead;
	}

	/// <summary>
	/// Create a copy of another stored item.
	/// </summary>
	public StoredItem(IReadOnlyStoredItem item) : this(item, item.Identifier, item.Storage, item.IsRead)
	{
	}
	
	public Guid Identifier { get; }
	
	public IItemStorage Storage { get; }

	public bool IsRead
	{
		get => _isRead;
		set => SetProperty(ref _isRead, value);
	}

	private bool _isRead;
}
