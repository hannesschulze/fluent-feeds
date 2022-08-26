using System;
using System.Threading;
using System.Threading.Tasks;
using FluentFeeds.App.Shared.Models.Storage;
using FluentFeeds.App.Shared.Resources;
using FluentFeeds.Feeds.Base.Items.Content;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace FluentFeeds.App.Shared.Models.Items;

/// <summary>
/// Stored representation of an item.
/// </summary>
public sealed class Item : ObservableObject, IItemView
{
	public Item(
		Guid identifier, IItemStorage storage, string? userIdentifier, string title, string? author, string? summary,
		DateTimeOffset publishedTimestamp, DateTimeOffset modifiedTimestamp, Uri? url, Uri? contentUrl, bool isRead,
		IItemContentLoader contentLoader)
	{
		Identifier = identifier;
		Storage = storage;
		UserIdentifier = userIdentifier;
		_title = title;
		_author = author;
		_summary = summary;
		_displayAuthor = GetDisplayAuthor();
		_displaySummary = GetDisplaySummary();
		_publishedTimestamp = publishedTimestamp;
		_modifiedTimestamp = modifiedTimestamp;
		_url = url;
		_contentUrl = contentUrl;
		_isRead = isRead;
		_contentLoader = contentLoader;
	}
	
	public Guid Identifier { get; }
	
	public IItemStorage Storage { get; }
	
	public string? UserIdentifier { get; }

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

	public bool IsRead
	{
		get => _isRead;
		set => SetProperty(ref _isRead, value);
	}

	public IItemContentLoader ContentLoader
	{
		get => _contentLoader;
		set => SetProperty(ref _contentLoader, value);
	}
	
	public Task<ItemContent> LoadContentAsync(bool reload = false, CancellationToken cancellation = default) =>
		ContentLoader.LoadAsync(reload, cancellation);
	
	private string GetDisplayAuthor() => Author ?? LocalizedStrings.FallbackItemAuthor;

	private string GetDisplaySummary() => Summary ?? LocalizedStrings.FallbackItemSummary;

	private string _title;
	private string? _author;
	private string? _summary;
	private string _displayAuthor;
	private string _displaySummary;
	private DateTimeOffset _publishedTimestamp;
	private DateTimeOffset _modifiedTimestamp;
	private Uri? _url;
	private Uri? _contentUrl;
	private bool _isRead;
	private IItemContentLoader _contentLoader;
}
