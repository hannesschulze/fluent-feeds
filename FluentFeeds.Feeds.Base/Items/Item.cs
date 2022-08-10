using System;
using FluentFeeds.Feeds.Base.Items.Content;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace FluentFeeds.Feeds.Base.Items;

/// <summary>
/// A mutable item.
/// </summary>
public class Item : ObservableObject, IReadOnlyItem
{
	public Item(
		Uri url, Uri? contentUrl, DateTimeOffset publishedTimestamp, DateTimeOffset modifiedTimestamp, string title,
		string author, string? summary, ItemContent content)
	{
		_url = url;
		_contentUrl = contentUrl;
		_publishedTimestamp = publishedTimestamp;
		_modifiedTimestamp = modifiedTimestamp;
		_title = title;
		_author = author;
		_summary = summary;
		_content = content;
	}

	public Uri Url
	{
		get => _url;
		set => SetProperty(ref _url, value);
	}

	public Uri? ContentUrl
	{
		get => _contentUrl;
		set => SetProperty(ref _contentUrl, value);
	}

	public DateTimeOffset PublishedTimestamp
	{
		get => _publishedTimestamp;
		set => SetProperty(ref _publishedTimestamp, value);
	}

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

	public ItemContent Content
	{
		get => _content;
		set => SetProperty(ref _content, value);
	}

	private Uri _url;
	private Uri? _contentUrl;
	private DateTimeOffset _publishedTimestamp;
	private DateTimeOffset _modifiedTimestamp;
	private string _title;
	private string _author;
	private string? _summary;
	private ItemContent _content;
}
