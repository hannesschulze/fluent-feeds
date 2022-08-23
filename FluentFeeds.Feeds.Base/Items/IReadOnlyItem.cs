using System;
using System.ComponentModel;
using System.Threading.Tasks;
using FluentFeeds.Feeds.Base.Items.Content;
using FluentFeeds.Feeds.Base.Items.ContentLoaders;

namespace FluentFeeds.Feeds.Base.Items;

/// <summary>
/// Read-only view into an item.
/// </summary>
public interface IReadOnlyItem : INotifyPropertyChanging, INotifyPropertyChanged
{
	/// <summary>
	/// URL to the item itself.
	/// </summary>
	Uri? Url { get; }
	
	/// <summary>
	/// URL to the content of the item (if the content is not part of the item).
	/// </summary>
	Uri? ContentUrl { get; }

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
	/// An object used to dynamically load the item's content.
	/// </summary>
	IItemContentLoader ContentLoader { get; }

	/// <summary>
	/// Asynchronously load the item's content.
	/// </summary>
	Task<ItemContent> LoadContentAsync(bool reload = false);
}
