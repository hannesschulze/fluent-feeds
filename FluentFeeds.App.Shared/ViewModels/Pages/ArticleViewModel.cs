using System;
using System.ComponentModel;
using FluentFeeds.App.Shared.Models;
using FluentFeeds.App.Shared.Models.Items;
using FluentFeeds.App.Shared.Models.Navigation;
using FluentFeeds.App.Shared.Resources;
using FluentFeeds.App.Shared.Services;
using FluentFeeds.Documents;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace FluentFeeds.App.Shared.ViewModels.Pages;

/// <summary>
/// View model for a page displaying article content.
/// </summary>
public sealed class ArticleViewModel : ObservableObject
{
	public ArticleViewModel(ISettingsService settingsService)
	{
		_settingsService = settingsService;

		_fontFamily = _settingsService.ContentFontFamily;
		_fontSize = _settingsService.ContentFontSize;
		_settingsService.PropertyChanged += HandleSettingsChanged;
	}

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
	/// Font family used to display the content.
	/// </summary>
	public FontFamily FontFamily
	{
		get => _fontFamily;
		private set => SetProperty(ref _fontFamily, value);
	}

	/// <summary>
	/// Font size used to display the content.
	/// </summary>
	public FontSize FontSize
	{
		get => _fontSize;
		private set => SetProperty(ref _fontSize, value);
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
				? String.Format(LocalizedStrings.ItemInfoWithAuthor, _item.Author, publishedTimestamp.ToString("f"))
				: String.Format(LocalizedStrings.ItemInfoWithoutAuthor, publishedTimestamp.ToString("f"));
		}
	}

	private void HandleItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		switch (e.PropertyName)
		{
			case nameof(IItemView.Title):
				UpdateTitle();
				break;
			case nameof(IItemView.Author):
			case nameof(IItemView.PublishedTimestamp):
				UpdateItemInfo();
				break;
		}
	}

	private void HandleSettingsChanged(object? sender, PropertyChangedEventArgs e)
	{
		switch (e.PropertyName)
		{
			case nameof(ISettingsService.ContentFontFamily):
				FontFamily = _settingsService.ContentFontFamily;
				break;
			case nameof(ISettingsService.ContentFontSize):
				FontSize = _settingsService.ContentFontSize;
				break;
		}
	}

	private readonly ISettingsService _settingsService;
	private FontFamily _fontFamily;
	private FontSize _fontSize;
	private IItemView? _item;
	private string _title = String.Empty;
	private string _itemInfo = String.Empty;
	private RichText _content = new();
}
