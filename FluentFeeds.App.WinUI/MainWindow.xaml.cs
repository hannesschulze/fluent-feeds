using System;
using FluentFeeds.App.Shared.Models;
using FluentFeeds.App.Shared.Services;
using FluentFeeds.App.WinUI.Helpers;
using FluentFeeds.App.WinUI.Services;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;

namespace FluentFeeds.App.WinUI;

public sealed partial class MainWindow : Window
{
	public MainWindow()
	{
		InitializeComponent();

		if (Ioc.Default.GetService<IModalService>() is ModalService modalService)
		{
			modalService.XamlRootLocator = () => Content.XamlRoot;
		}

		_settingsService = Ioc.Default.GetRequiredService<ISettingsService>();
		_settingsService.PropertyChanged += HandleSettingsChanged;
		UpdateTheme();

		Title = MainPage.ViewModel.Title;
		this.GetAppWindow().SetIcon(MainPage.WindowIcon);

		if (MicaController.IsSupported())
		{
			_micaHelper = new MicaHelper(this);
		}

		if (AppWindowTitleBar.IsCustomizationSupported())
		{
			_titleBarHelper = new TitleBarHelper(this, () => MainPage.ComputeTitleBarDragRegions());
			MainPage.CaptionButtonsWidth = _titleBarHelper.CaptionButtonsWidth;
			MainPage.TitleBarHeight = _titleBarHelper.TitleBarHeight;
			MainPage.DragRegionSizeChanged += (s, e) => _titleBarHelper.UpdateDragRegions();
		}
	}

	private void HandleSettingsChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
	{
		switch (e.PropertyName)
		{
			case nameof(ISettingsService.AppTheme):
				UpdateTheme();
				break;
		}
	}

	private void UpdateTheme()
	{
		if (Content is FrameworkElement content)
		{
			content.RequestedTheme =
				_settingsService.AppTheme switch
				{
					Theme.SystemDefault => ElementTheme.Default,
					Theme.Light => ElementTheme.Light,
					Theme.Dark => ElementTheme.Dark,
					_ => throw new IndexOutOfRangeException()
				};
		}
	}

	private readonly ISettingsService _settingsService;
	private MicaHelper? _micaHelper;
	private TitleBarHelper? _titleBarHelper;
}
