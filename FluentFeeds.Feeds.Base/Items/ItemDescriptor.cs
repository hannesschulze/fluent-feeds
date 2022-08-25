using System;
using FluentFeeds.Feeds.Base.Feeds.Content;
using FluentFeeds.Feeds.Base.Items.Content;

namespace FluentFeeds.Feeds.Base.Items;

/// <summary>
/// An immutable descriptor which can be used to create or update an item in a feed.
/// </summary>
public sealed class ItemDescriptor : IEquatable<ItemDescriptor>
{
	public ItemDescriptor(
		string? identifier, string title, string? author, string? summary, DateTimeOffset publishedTimestamp,
		DateTimeOffset modifiedTimestamp, Uri? url, Uri? contentUrl, IItemContentLoader contentLoader)
	{
		Identifier = identifier;
		Title = title;
		Author = author;
		Summary = summary;
		PublishedTimestamp = publishedTimestamp;
		ModifiedTimestamp = modifiedTimestamp;
		Url = url;
		ContentUrl = contentUrl;
		ContentLoader = contentLoader;
	}
	
	/// <summary>
	/// A string which can be used to identify the item.
	/// </summary>
	public string? Identifier { get; }
	
	/// <summary>
	/// Title of the item.
	/// </summary>
	public string Title { get; }
	
	/// <summary>
	/// Name of the author who published the item.
	/// </summary>
	public string? Author { get; }
	
	/// <summary>
	/// Summary of the item content (usually a short excerpt formatted as plain text).
	/// </summary>
	public string? Summary { get; }

	/// <summary>
	/// The timestamp at which the item was published.
	/// </summary>
	public DateTimeOffset PublishedTimestamp { get; }

	/// <summary>
	/// The timestamp at which the item was last modified.
	/// </summary>
	public DateTimeOffset ModifiedTimestamp { get; }
	
	/// <summary>
	/// URL to the item itself.
	/// </summary>
	public Uri? Url { get; }
	
	/// <summary>
	/// URL to the content of the item (if the content is not part of the item).
	/// </summary>
	public Uri? ContentUrl { get; }
	
	/// <summary>
	/// An object used to dynamically load the item's content.
	/// </summary>
	public IItemContentLoader ContentLoader { get; }
	
	public bool Equals(ItemDescriptor? other)
	{
		if (ReferenceEquals(this, other))
			return true;
		if (ReferenceEquals(null, other))
			return false;
		return 
			Identifier == other.Identifier && Title == other.Title && Author == other.Author && 
			Summary == other.Summary && PublishedTimestamp == other.PublishedTimestamp && 
			ModifiedTimestamp == other.ModifiedTimestamp && Url == other.Url && ContentUrl == other.ContentUrl && 
			ContentLoader == other.ContentLoader;
	}

	public override bool Equals(object? obj)
	{
		if (ReferenceEquals(this, obj))
			return true;
		return obj is ItemDescriptor other && Equals(other);
	}
	
	public override int GetHashCode()
	{
		return HashCode.Combine(
			Identifier, HashCode.Combine(
				Title, Author, Summary, PublishedTimestamp, ModifiedTimestamp, Url, ContentUrl, ContentLoader));
	}

	public override string ToString() =>
		$"ItemDescriptor {{ Identifier = {Identifier}, Title = {Title}, Author = {Author}, Summary = {Summary}, " +
		$"PublishedTimestamp = {PublishedTimestamp}, ModifiedTimestamp = {ModifiedTimestamp}, Url = {Url}, " +
		$"ContentUrl = {ContentUrl}, ContentLoader = {ContentLoader} }}";

	public static bool operator ==(ItemDescriptor? lhs, ItemDescriptor? rhs) => lhs?.Equals(rhs) ?? ReferenceEquals(rhs, null);
	
	public static bool operator !=(ItemDescriptor? lhs, ItemDescriptor? rhs) => !(lhs == rhs);
}
