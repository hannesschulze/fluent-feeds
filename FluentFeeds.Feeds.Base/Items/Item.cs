using System;
using System.Threading.Tasks;
using FluentFeeds.Feeds.Base.Items.Content;
using FluentFeeds.Feeds.Base.Items.ContentLoaders;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace FluentFeeds.Feeds.Base.Items;

/// <summary>
/// A mutable item.
/// </summary>
public class Item : ObservableObject, IReadOnlyItem
{
	public Item(
		Uri? url, Uri? contentUrl, DateTimeOffset publishedTimestamp, DateTimeOffset modifiedTimestamp, string title,
		string? author, string? summary, IItemContentLoader contentLoader)
	{
		_url = url;
		_contentUrl = contentUrl;
		_publishedTimestamp = publishedTimestamp;
		_modifiedTimestamp = modifiedTimestamp;
		_title = title;
		_author = author;
		_summary = summary;
		_displayAuthor = GetDisplayAuthor();
		_displaySummary = GetDisplaySummary();
		_contentLoader = contentLoader;
	}

	/// <summary>
	/// Create a copy of another item.
	/// </summary>
	public Item(IReadOnlyItem item) : this(
		item.Url, item.ContentUrl, item.PublishedTimestamp, item.ModifiedTimestamp, item.Title, item.Author,
		item.Summary, item.ContentLoader)
	{
	}

	public Uri? Url
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

	public string? Author
	{
		get => _author;
		set
		{
			if (SetProperty(ref _author, value))
			{
				DisplayAuthor = GetDisplayAuthor();
			}
		}
	}

	public string? Summary
	{
		get => _summary;
		set
		{
			if (SetProperty(ref _summary, value))
			{
				DisplaySummary = GetDisplaySummary();
			}
		}
	}

	public string DisplayAuthor
	{
		get => _displayAuthor;
		private set => SetProperty(ref _displayAuthor, value);
	}

	public string DisplaySummary
	{
		get => _displaySummary;
		private set => SetProperty(ref _displaySummary, value);
	}

	public IItemContentLoader ContentLoader
	{
		get => _contentLoader;
		set => SetProperty(ref _contentLoader, value);
	}

	public Task<ItemContent> LoadContentAsync(bool reload = false) => ContentLoader.LoadAsync(reload);

	private string GetDisplayAuthor() => Author ?? "Unknown author";

	private string GetDisplaySummary() => Summary ?? "This item does not have a summary.";

	private Uri? _url;
	private Uri? _contentUrl;
	private DateTimeOffset _publishedTimestamp;
	private DateTimeOffset _modifiedTimestamp;
	private string _title;
	private string? _author;
	private string? _summary;
	private string _displayAuthor;
	private string _displaySummary;
	private IItemContentLoader _contentLoader;
}
