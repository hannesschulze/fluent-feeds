using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using FluentFeeds.App.Shared.ViewModels;
using FluentFeeds.App.WinUI.Helpers;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Windows.ApplicationModel;

namespace FluentFeeds.App.WinUI.Pages;

/// <summary>
/// The main page of the app managing the navigation view and the titlebar.
/// </summary>
public sealed partial class MainPage : Page
{
	private double SearchFieldMaxWidth => 500;
	private double NavigationExpandedWidth => 260;
	private double NavigationCompactWidth => 48;
	private double NavigationCompactTitleInset => 20;
	private double MinWidthForTitleVisible => NavigationCompactWidth + CaptionButtonsWidth + SearchFieldMaxWidth;
	private double MinWidthForSearchSpacing => NavigationExpandedWidth + CaptionButtonsWidth + SearchFieldMaxWidth;
	private double MinWidthForCenterSearch => NavigationExpandedWidth * 2 + SearchFieldMaxWidth;
	private GridLength SearchFieldMaxGridLength => new(SearchFieldMaxWidth);
	private GridLength NavigationCompactGridLength => new(NavigationCompactWidth);
	private GridLength NavigationExpandedGridLength => new(NavigationExpandedWidth);
	private GridLength CaptionButtonsGridLength => new(CaptionButtonsWidth);
	private Thickness TitleBarAreaLeftMargin => new(left: NavigationCompactWidth, 0, 0, 0);
	private Thickness ContentFrameMargin => new(0, top: TitleBarHeight, 0, 0);
	private ObservableCollection<NavigationItemViewModel> FooterItems { get; } = new();

	public MainViewModel ViewModel => (MainViewModel)DataContext;

	public string WindowTitle => Package.Current.DisplayName;
	public string WindowIcon => Path.Combine(Package.Current.InstalledLocation.Path, "Assets", "WindowIcon.ico");

	public double CaptionButtonsWidth { get; set; } = 0;
	public double TitleBarHeight { get; set; } = 48;

	/// <summary>
	/// Event invoked when the size of the drag regions might have changed.
	/// </summary>
	public event EventHandler<EventArgs>? DragRegionSizeChanged;

	public MainPage()
	{
		DataContext = Ioc.Default.GetRequiredService<MainViewModel>();
		FooterItems.Add(ViewModel.SettingsItem);
		InitializeComponent();

		_titleBar.Loaded += (s, e) => DragRegionSizeChanged?.Invoke(this, EventArgs.Empty);
		_navigationView.PaneClosing += (s, e) => UpdateTitleInset();
		_navigationView.PaneOpening += (s, e) => UpdateTitleInset();
		_navigationView.DisplayModeChanged += (s, e) => UpdateTitleInset();
		UpdateTitleInset();

		// XXX: NavigationView does not support binding commands to the back button yet:
		// https://github.com/microsoft/microsoft-ui-xaml/issues/944
		_navigationView.BackRequested += (s, e) => ViewModel.GoBackCommand.Execute(null);
		ViewModel.GoBackCommand.CanExecuteChanged += (s, e) => UpdateBackButtonEnabled();
		ViewModel.PropertyChanged += HandlePropertyChanged;
		UpdateBackButtonEnabled();
		UpdateVisiblePage();
	}

	private void HandlePropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		switch (e.PropertyName)
		{
			case nameof(MainViewModel.VisiblePage):
				UpdateVisiblePage();
				break;
		}
	}

	private void HandleDragRegionSizeChanged(object sender, SizeChangedEventArgs e) =>
		DragRegionSizeChanged?.Invoke(this, EventArgs.Empty);

	private void UpdateBackButtonEnabled() =>
		_navigationView.IsBackEnabled = ViewModel.GoBackCommand.CanExecute(null);

	private void UpdateVisiblePage() =>
		_contentFrame.Navigate(
			ViewModel.VisiblePage switch
			{
				MainViewModel.Page.Settings => typeof(SettingsPage),
				MainViewModel.Page.Feed => typeof(FeedPage),
				_ => throw new IndexOutOfRangeException()
			},
			null,
			new EntranceNavigationTransitionInfo());

	/// <summary>
	/// Update the title's margin. Spacing is added before the title when the navigation bar is collapsed.
	/// </summary>
	private void UpdateTitleInset()
	{
		var hasInset = 
			_navigationView.DisplayMode != NavigationViewDisplayMode.Expanded || !_navigationView.IsPaneOpen;
		_titleBarTitle.Margin = new Thickness(left: hasInset ? NavigationCompactTitleInset : 0, 0, 0, 0);
	}

	/// <summary>
	/// Compute the title bar drag regions for the current window size.
	/// </summary>
	public TitleBarDragRegions ComputeTitleBarDragRegions() => new(
		LeftStart: NavigationCompactWidth,
		LeftWidth: _titleBarAreaLeftColumn.ActualWidth - NavigationCompactWidth + 8,
		RightStart: _titleBarAreaLeftColumn.ActualWidth + _titleBarAreaCenterColumn.ActualWidth - 8,
		RightWidth: _titleBarAreaRightColumn.ActualWidth + 8);
}
