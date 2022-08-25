using FluentFeeds.App.Shared.Models;
using FluentFeeds.App.Shared.ViewModels.Pages;
using FluentFeeds.App.WinUI.Helpers;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;

namespace FluentFeeds.App.WinUI.Views.Pages;

/// <summary>
/// Page for adjusting application settings.
/// </summary>
public sealed partial class SettingsPage : Page
{
	public SettingsPage()
	{
		DataContext = Ioc.Default.GetRequiredService<SettingsViewModel>();
		InitializeComponent();

		AppThemeSymbol = Common.Symbol.ColorPalette.ToIconElement();
		SelectThemeCommand = new RelayCommand<Theme>(theme => ViewModel.SelectedTheme = theme);
	}

	public SettingsViewModel ViewModel => (SettingsViewModel)DataContext;

	private IconElement AppThemeSymbol { get; }

	private RelayCommand<Theme> SelectThemeCommand { get; }

	private bool IsThemeSelected(Theme theme, Theme itemTheme) => theme == itemTheme;
}
