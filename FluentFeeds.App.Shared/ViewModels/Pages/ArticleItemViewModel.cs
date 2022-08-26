using System;
using System.ComponentModel;
using FluentFeeds.App.Shared.Models;
using FluentFeeds.App.Shared.Models.Navigation;
using FluentFeeds.App.Shared.Services;
using FluentFeeds.Documents;

namespace FluentFeeds.App.Shared.ViewModels.Pages;

/// <summary>
/// View model for a page displaying an item with article content.
/// </summary>
public sealed class ArticleItemViewModel : ItemViewModel
{
	public ArticleItemViewModel(ISettingsService settingsService)
	{
		_settingsService = settingsService;
		_settingsService.PropertyChanged += HandleSettingsChanged;
	}

	public override void Load(FeedNavigationRoute route)
	{
		if (route.Type != FeedNavigationRouteType.ArticleItem)
			throw new Exception("Invalid route type.");

		base.Load(route);
		Content = route.ArticleContent!.Body;
	}

	/// <summary>
	/// Font family used to display the content.
	/// </summary>
	public FontFamily FontFamily => _settingsService.ContentFontFamily;

	/// <summary>
	/// Font size used to display the content.
	/// </summary>
	public FontSize FontSize => _settingsService.ContentFontSize;

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

	protected override void UpdateTitle(string title)
	{
		Title = title;
	}

	protected override void UpdateItemInfo(string itemInfo)
	{
		ItemInfo = itemInfo;
	}

	private void HandleSettingsChanged(object? sender, PropertyChangedEventArgs e)
	{
		switch (e.PropertyName)
		{
			case nameof(ISettingsService.ContentFontFamily):
				OnPropertyChanged(nameof(FontFamily));
				break;
			case nameof(ISettingsService.ContentFontSize):
				OnPropertyChanged(nameof(FontSize));
				break;
		}
	}

	private readonly ISettingsService _settingsService;
	private RichText _content = new();
	private string _title = String.Empty;
	private string _itemInfo = String.Empty;
}
