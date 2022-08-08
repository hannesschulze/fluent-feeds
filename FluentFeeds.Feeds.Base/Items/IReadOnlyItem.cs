using System;
using System.ComponentModel;

namespace FluentFeeds.Feeds.Base.Items;

/// <summary>
/// Read-only view into an <see cref="Item"/>.
/// </summary>
public interface IReadOnlyItem : INotifyPropertyChanging, INotifyPropertyChanged
{
	/// <summary>
	/// Accept a visitor for this item object.
	/// </summary>
	void Accept(IItemVisitor visitor);
	
	/// <summary>
	/// The type of this item.
	/// </summary>
	ItemType Type { get; }
	
	/// <summary>
	/// Unique identifier for this item.
	/// </summary>
	Guid Identifier { get; }
	
	/// <summary>
	/// URL to the item itself.
	/// </summary>
	Uri Url { get; }

	/// <summary>
	/// The timestamp at which the item was published.
	/// </summary>
	DateTimeOffset PublishedTimestamp { get; }

	/// <summary>
	/// The timestamp at which the item was last modified.
	/// </summary>
	DateTimeOffset ModifiedTimestamp { get; }
	
	/// <summary>
	/// Title of the item.
	/// </summary>
	string Title { get; }
	
	/// <summary>
	/// Name of the author who published the item.
	/// </summary>
	string Author { get; }
	
	/// <summary>
	/// Summary of the article (usually a short excerpt formatted as plain text).
	/// </summary>
	string? Summary { get; }
	
	/// <summary>
	/// URL to the content of the item (if the content is not part of the item).
	/// </summary>
	Uri? ContentUrl { get; }
	
	/// <summary>
	/// Flag indicating whether the item was read.
	/// </summary>
	bool IsRead { get; }
}
