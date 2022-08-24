using System;
using System.ComponentModel;
using System.Threading.Tasks;
using FluentFeeds.App.Shared.Models.Storage;
using FluentFeeds.Feeds.Base.Items.Content;

namespace FluentFeeds.App.Shared.Models.Items;

/// <summary>
/// Read-only view into a stored item.
/// </summary>
public interface IItemView : INotifyPropertyChanged
{
	/// <summary>
	/// Unique identifier for this item.
	/// </summary>
	Guid Identifier { get; }
	
	/// <summary>
	/// Storage managing this item.
	/// </summary>
	IItemStorage Storage { get; }
	
	/// <summary>
	/// A string which can be used to identify the item in the storage, provided by the feed content loader.
	/// </summary>
	string? UserIdentifier { get; }
	
	/// <summary>
	/// Title of the item.
	/// </summary>
	string Title { get; }
	
	/// <summary>
	/// Name of the author who published the item.
	/// </summary>
	string? Author { get; }
	
	/// <summary>
	/// Summary of the item content (usually a short excerpt formatted as plain text).
	/// </summary>
	string? Summary { get; }

	/// <summary>
	/// The displayed author, using a fallback if <see cref="Author"/> is not set.
	/// </summary>
	string DisplayAuthor { get; }

	/// <summary>
	/// The displayed summary, using a fallback if <see cref="Summary"/> is not set.
	/// </summary>
	string DisplaySummary { get; }

	/// <summary>
	/// The timestamp at which the item was published.
	/// </summary>
	DateTimeOffset PublishedTimestamp { get; }

	/// <summary>
	/// The timestamp at which the item was last modified.
	/// </summary>
	DateTimeOffset ModifiedTimestamp { get; }
	
	/// <summary>
	/// URL to the item itself.
	/// </summary>
	Uri? Url { get; }
	
	/// <summary>
	/// URL to the content of the item (if the content is not part of the item).
	/// </summary>
	Uri? ContentUrl { get; }
	
	/// <summary>
	/// Flag indicating whether the item was read by the user.
	/// </summary>
	bool IsRead { get; }

	/// <summary>
	/// An object used to dynamically load the item's content.
	/// </summary>
	IItemContentLoader ContentLoader { get; }

	/// <summary>
	/// Asynchronously load the item's content.
	/// </summary>
	Task<ItemContent> LoadContentAsync(bool reload = false);
}
