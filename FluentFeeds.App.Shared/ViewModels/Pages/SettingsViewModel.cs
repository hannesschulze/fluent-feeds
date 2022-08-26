using System;
using System.ComponentModel;
using System.Windows.Input;
using FluentFeeds.App.Shared.Models;
using FluentFeeds.App.Shared.Resources;
using FluentFeeds.App.Shared.Services;
using FluentFeeds.Common;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;

namespace FluentFeeds.App.Shared.ViewModels.Pages;

/// <summary>
/// View-model for the app settings page.
/// </summary>
public sealed class SettingsViewModel : ObservableObject
{
	public SettingsViewModel(ISettingsService settingsService, IWebBrowserService webBrowserService)
	{
		_settingsService = settingsService;
		_settingsService.PropertyChanged += HandlePropertyChanged;
		_webBrowserService = webBrowserService;
		_openProjectWebsiteCommand = new RelayCommand(HandleOpenProjectWebsiteCommand);
		_openGitHubIssuesCommand = new RelayCommand(HandleOpenGitHubIssuesCommand);
	}

	/// <summary>
	/// Open the app project website in the user's default web browser.
	/// </summary>
	public ICommand OpenProjectWebsiteCommand => _openProjectWebsiteCommand;

	/// <summary>
	/// Open the GitHub issues page for the app in the user's default browser.
	/// </summary>
	public ICommand OpenGitHubIssuesCommand => _openGitHubIssuesCommand;

	/// <summary>
	/// Page title.
	/// </summary>
	public string Title => LocalizedStrings.SettingsTitle;

	/// <summary>
	/// Heading for the "appearance" section.
	/// </summary>
	public string AppearanceHeading => LocalizedStrings.SettingsAppearanceHeading;

	/// <summary>
	/// Heading for the "about this app" section.
	/// </summary>
	public string AboutHeading => LocalizedStrings.SettingsAboutHeading;

	/// <summary>
	/// Symbol for the <see cref="SelectedTheme"/> menu.
	/// </summary>
	public Symbol ThemeSymbol => Symbol.ColorPalette;

	/// <summary>
	/// Label for the <see cref="SelectedTheme"/> menu.
	/// </summary>	
	public string ThemeLabel => LocalizedStrings.SettingsThemeLabel;
	
	/// <summary>
	/// Description for the <see cref="SelectedTheme"/> menu.
	/// </summary>	
	public string ThemeDescription => LocalizedStrings.SettingsThemeDescription;

	/// <summary>
	/// Menu label for <see cref="Theme.Light"/>.
	/// </summary>
	public string ThemeLightLabel => LocalizedStrings.SettingsThemeLightLabel;

	/// <summary>
	/// Menu label for <see cref="Theme.Dark"/>.
	/// </summary>
	public string ThemeDarkLabel => LocalizedStrings.SettingsThemeDarkLabel;

	/// <summary>
	/// Menu label for <see cref="Theme.SystemDefault"/>.
	/// </summary>
	public string ThemeSystemDefaultLabel => LocalizedStrings.SettingsThemeSystemDefaultLabel;

	/// <summary>
	/// Text displayed in the "about" section.
	/// </summary>
	public string AboutText => $"{Constants.AppName} {Constants.AppVersion}";

	/// <summary>
	/// Label for <see cref="OpenProjectWebsiteCommand"/>.
	/// </summary>
	public string OpenProjectWebsiteLabel => LocalizedStrings.SettingsOpenProjectWebsiteLabel;

	/// <summary>
	/// Label for <see cref="OpenGitHubIssuesCommand"/>.
	/// </summary>
	public string OpenGitHubIssuesLabel => LocalizedStrings.OpenGitHubIssuesLabel;

	/// <summary>
	/// The currently selected application theme.
	/// </summary>
	public Theme SelectedTheme
	{
		get => _settingsService.AppTheme;
		set => _settingsService.AppTheme = value;
	}

	private void HandlePropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		switch (e.PropertyName)
		{
			case nameof(ISettingsService.AppTheme):
				OnPropertyChanged(nameof(SelectedTheme));
				break;
		}
	}

	private void HandleOpenProjectWebsiteCommand()
	{
		_webBrowserService.Open(new Uri(Constants.AppWebsiteUrl));
	}

	private void HandleOpenGitHubIssuesCommand()
	{
		_webBrowserService.Open(new Uri(Constants.AppIssuesUrl));
	}

	private readonly ISettingsService _settingsService;
	private readonly IWebBrowserService _webBrowserService;
	private readonly RelayCommand _openProjectWebsiteCommand;
	private readonly RelayCommand _openGitHubIssuesCommand;
}
