using System;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace FluentFeeds.Feeds.Base.Items;

/// <summary>
/// An item is part of a feed and provides content in a particular form.
/// </summary>
public abstract class Item : ObservableObject, IReadOnlyItem
{
	protected Item(
		Guid identifier, Uri url, DateTimeOffset publishedTimestamp, DateTimeOffset modifiedTimestamp, string title,
		string author, string? summary = null, Uri? contentUrl = null, bool isRead = false)
	{
		Identifier = identifier;
		Url = url;
		PublishedTimestamp = publishedTimestamp;
		_modifiedTimestamp = modifiedTimestamp;
		_title = title;
		_author = author;
		_summary = summary;
		_contentUrl = contentUrl;
		_isRead = isRead;
	}

	public abstract void Accept(IItemVisitor visitor);
	
	public abstract ItemType Type { get; }
	
	public Guid Identifier { get; }
	
	public Uri Url { get; }

	public DateTimeOffset PublishedTimestamp { get; }

	public DateTimeOffset ModifiedTimestamp
	{
		get => _modifiedTimestamp;
		set => SetProperty(ref _modifiedTimestamp, value);
	}

	public string Title
	{
		get => _title;
		set => SetProperty(ref _title, value);
	}

	public string Author
	{
		get => _author;
		set => SetProperty(ref _author, value);
	}

	public string? Summary
	{
		get => _summary;
		set => SetProperty(ref _summary, value);
	}

	public Uri? ContentUrl
	{
		get => _contentUrl;
		set => SetProperty(ref _contentUrl, value);
	}

	public bool IsRead
	{
		get => _isRead;
		set => SetProperty(ref _isRead, value);
	}

	private DateTimeOffset _modifiedTimestamp;
	private string _title;
	private string _author;
	private string? _summary;
	private Uri? _contentUrl;
	private bool _isRead;
}
