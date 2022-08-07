using System;

namespace FluentFeeds.Shared.Models.Items;

/// <summary>
/// An item is part of a feed and provides content in a particular form.
/// </summary>
public abstract class Item
{
	public Item(
		DateTimeOffset timestamp, string title, string author, Uri url,
		string? summary = null, Uri? contentUrl = null, bool isRead = false)
	{
		Timestamp = timestamp;
		Title = title;
		Author = author;
		Summary = summary;
		Url = url;
		ContentUrl = contentUrl;
		IsRead = isRead;
	}

	/// <summary>
	/// Accept a visitor for this item object.
	/// </summary>
	public abstract void Accept(IItemVisitor visitor);
	
	/// <summary>
	/// The type of this item.
	/// </summary>
	public abstract ItemType Type { get; }

	/// <summary>
	/// The timestamp at which the item was published.
	/// </summary>
	public DateTimeOffset Timestamp { get; }
	
	/// <summary>
	/// Title of the item.
	/// </summary>
	public string Title { get; }
	
	/// <summary>
	/// Name of the author who published the item.
	/// </summary>
	public string Author { get; }
	
	/// <summary>
	/// URL to the item itself.
	/// </summary>
	public Uri Url { get; }
	
	/// <summary>
	/// Summary of the article (usually a short excerpt formatted as plain text).
	/// </summary>
	public string? Summary { get; }
	
	/// <summary>
	/// URL to the content of the item (if the content is not part of the item).
	/// </summary>
	public Uri? ContentUrl { get; }
	
	/// <summary>
	/// Flag indicating whether the item was read.
	/// </summary>
	public bool IsRead { get; }
}
