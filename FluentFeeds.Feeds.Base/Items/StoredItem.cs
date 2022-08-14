using System;
using FluentFeeds.Feeds.Base.Items.ContentLoaders;

namespace FluentFeeds.Feeds.Base.Items;

/// <summary>
/// <para>A mutable representation of a persistently stored item.</para>
///
/// <para>This adds a unique identifier and a "read" flag to the base item.</para>
/// </summary>
public class StoredItem : Item, IReadOnlyStoredItem
{
	public StoredItem(
		Guid identifier, Uri? url, Uri? contentUrl, DateTimeOffset publishedTimestamp, DateTimeOffset modifiedTimestamp,
		string title, string? author, string? summary, IItemContentLoader contentLoader, bool isRead)
		: base(url, contentUrl, publishedTimestamp, modifiedTimestamp, title, author, summary, contentLoader)
	{
		Identifier = identifier;
		_isRead = isRead;
	}
	
	/// <summary>
	/// Create a stored item from a base item.
	/// </summary>
	public StoredItem(IReadOnlyItem item, Guid identifier, bool isRead) : this(
		identifier, item.Url, item.ContentUrl, item.PublishedTimestamp, item.ModifiedTimestamp, item.Title, item.Author,
		item.Summary,item.ContentLoader, isRead)
	{
	}
	
	public Guid Identifier { get; }

	public bool IsRead
	{
		get => _isRead;
		set => SetProperty(ref _isRead, value);
	}

	private bool _isRead;
}
