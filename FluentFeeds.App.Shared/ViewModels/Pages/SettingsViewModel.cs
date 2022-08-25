using System.ComponentModel;
using FluentFeeds.App.Shared.Models;
using FluentFeeds.App.Shared.Services;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace FluentFeeds.App.Shared.ViewModels.Pages;

/// <summary>
/// View-model for the app settings page.
/// </summary>
public sealed class SettingsViewModel : ObservableObject
{
	public SettingsViewModel(ISettingsService settingsService)
	{
		_settingsService = settingsService;
		_settingsService.PropertyChanged += HandlePropertyChanged;
	}

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

	private readonly ISettingsService _settingsService;
}
