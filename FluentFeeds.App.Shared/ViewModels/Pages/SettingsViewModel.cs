using System;
using System.ComponentModel;
using System.Windows.Input;
using FluentFeeds.App.Shared.Models;
using FluentFeeds.App.Shared.Services;
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
	/// The currently selected application theme.
	/// </summary>
	public Theme SelectedTheme
	{
		get => _settingsService.AppTheme;
		set => _settingsService.AppTheme = value;
	}

	/// <summary>
	/// Open the app project website in the user's default web browser.
	/// </summary>
	public ICommand OpenProjectWebsiteCommand => _openProjectWebsiteCommand;

	/// <summary>
	/// Open the GitHub issues page for the app in the user's default browser.
	/// </summary>
	public ICommand OpenGitHubIssuesCommand => _openGitHubIssuesCommand;

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
