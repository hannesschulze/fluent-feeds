using System;
using System.ComponentModel;
using System.Reflection.Metadata;
using FluentFeeds.App.Shared.Models.Navigation;
using FluentFeeds.Documents;
using FluentFeeds.Feeds.Base.Items;
using FluentFeeds.Feeds.Base.Items.Content;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace FluentFeeds.App.Shared.ViewModels.Pages;

/// <summary>
/// View model for a page displaying article content.
/// </summary>
public sealed class ArticleViewModel : ObservableObject
{
	/// <summary>
	/// Called after navigating to the article page.
	/// </summary>
	/// <param name="route">Route containing parameters.</param>
	public void Load(FeedNavigationRoute route)
	{
		if (route.Type != FeedNavigationRouteType.Article)
			throw new Exception("Invalid route type.");

		if (_item != null)
		{
			_item.PropertyChanged -= HandleItemPropertyChanged;
		}

		_item = route.Item!;
		_item.PropertyChanged += HandleItemPropertyChanged;
		Content = route.ArticleContent!.Body;
		UpdateTitle();
		UpdateItemInfo();
	}

	/// <summary>
	/// Title of the article.
	/// </summary>
	public string Title
	{
		get => _title;
		private set => SetProperty(ref _title, value);
	}
	
	/// <summary>
	/// Line of text shown below the title.
	/// </summary>
	public string ItemInfo
	{
		get => _itemInfo;
		private set => SetProperty(ref _itemInfo, value);
	}

	/// <summary>
	/// Main content of the article.
	/// </summary>
	public RichText Content
	{
		get => _content;
		private set => SetProperty(ref _content, value);
	}

	private void UpdateTitle()
	{
		if (_item != null)
		{
			Title = _item?.Title ?? String.Empty;
		}
	}

	private void UpdateItemInfo()
	{
		if (_item != null)
		{
			var publishedTimestamp = _item.PublishedTimestamp.ToLocalTime();
			ItemInfo = _item.Author != null
				? $"Published by {_item.Author} on {publishedTimestamp:f}"
				: $"Published on {publishedTimestamp:f}";
		}
	}

	private void HandleItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		switch (e.PropertyName)
		{
			case nameof(IReadOnlyItem.Title):
				UpdateTitle();
				break;
			case nameof(IReadOnlyItem.Author):
			case nameof(IReadOnlyItem.PublishedTimestamp):
				UpdateItemInfo();
				break;
		}
	}

	private IReadOnlyStoredItem? _item;
	private string _title = String.Empty;
	private string _itemInfo = String.Empty;
	private RichText _content = new();
}
