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

		_fontFamily = _settingsService.ContentFontFamily;
		_fontSize = _settingsService.ContentFontSize;
		_settingsService.PropertyChanged += HandleSettingsChanged;
	}

	public override void Load(FeedNavigationRoute route)
	{
		if (route.Type != FeedNavigationRouteType.ArticleItem)
			throw new Exception("Invalid route type.");

		Content = route.ArticleContent!.Body;
		base.Load(route);
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
	/// Main content of the article.
	/// </summary>
	public RichText Content
	{
		get => _content;
		private set => SetProperty(ref _content, value);
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
	private RichText _content = new();
	private FontFamily _fontFamily;
	private FontSize _fontSize;
}
