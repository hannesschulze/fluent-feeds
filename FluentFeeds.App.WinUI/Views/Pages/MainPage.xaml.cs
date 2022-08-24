using System;
using System.ComponentModel;
using System.IO;
using Windows.ApplicationModel;
using FluentFeeds.App.Shared.ViewModels.Pages;
using FluentFeeds.App.WinUI.Helpers;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using FluentFeeds.App.Shared.Services;
using FluentFeeds.App.WinUI.Services;
using FluentFeeds.App.Shared.Models.Navigation;

namespace FluentFeeds.App.WinUI.Views.Pages;

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
	private Thickness ContentMargin => new(0, top: TitleBarHeight, 0, 0);

	public MainPage()
	{
		DataContext = Ioc.Default.GetRequiredService<MainViewModel>();
		InitializeComponent();

		if (Ioc.Default.GetService<IModalService>() is ModalService modalService)
		{
			modalService.NavigationItemLocator =
				itemViewModel => NavigationView.ContainerFromMenuItem(itemViewModel) as FrameworkElement;
			modalService.ErrorBarLocator = () => ErrorBar;
		}

		TitleBar.Loaded += (s, e) => DragRegionSizeChanged?.Invoke(this, EventArgs.Empty);
		NavigationView.PaneClosing += (s, e) => UpdateTitleInset();
		NavigationView.PaneOpening += (s, e) => UpdateTitleInset();
		NavigationView.DisplayModeChanged += (s, e) => UpdateTitleInset();
		UpdateTitleInset();

		// XXX: NavigationView does not support binding commands to the back button yet:
		// https://github.com/microsoft/microsoft-ui-xaml/issues/944
		NavigationView.BackRequested += (s, e) => ViewModel.GoBackCommand.Execute(null);
		ViewModel.GoBackCommand.CanExecuteChanged += (s, e) => UpdateBackButtonEnabled();
		ViewModel.PropertyChanged += HandlePropertyChanged;
		UpdateBackButtonEnabled();
		UpdateCurrentRoute();
	}
	
	public MainViewModel ViewModel => (MainViewModel)DataContext;

	public string WindowTitle => Package.Current.DisplayName;
	public string WindowIcon => Path.Combine(Package.Current.InstalledLocation.Path, "Assets", "WindowIcon.ico");

	public double CaptionButtonsWidth { get; set; } = 0;
	public double TitleBarHeight { get; set; } = 48;

	/// <summary>
	/// Event invoked when the size of the drag regions might have changed.
	/// </summary>
	public event EventHandler<EventArgs>? DragRegionSizeChanged;

	private void HandlePropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		switch (e.PropertyName)
		{
			case nameof(MainViewModel.CurrentRoute):
				UpdateCurrentRoute();
				break;
		}
	}

	private void HandleDragRegionSizeChanged(object sender, SizeChangedEventArgs e) =>
		DragRegionSizeChanged?.Invoke(this, EventArgs.Empty);

	private void HandleErrorBarClosed(InfoBar sender, InfoBarClosedEventArgs args) =>
		ErrorBar.Margin = new Thickness(0);

	private void UpdateBackButtonEnabled() =>
		NavigationView.IsBackEnabled = ViewModel.GoBackCommand.CanExecute(null);

	private void UpdateCurrentRoute() =>
		ContentFrame.Navigate(
			ViewModel.CurrentRoute.Type switch
			{
				MainNavigationRouteType.Settings => typeof(SettingsPage),
				MainNavigationRouteType.Feed => typeof(FeedPage),
				_ => throw new IndexOutOfRangeException()
			}, ViewModel.CurrentRoute, new EntranceNavigationTransitionInfo());

	/// <summary>
	/// Update the title's margin. Spacing is added before the title when the navigation bar is collapsed.
	/// </summary>
	private void UpdateTitleInset()
	{
		var hasInset = 
			NavigationView.DisplayMode != NavigationViewDisplayMode.Expanded || !NavigationView.IsPaneOpen;
		TitleBarTitle.Margin = new Thickness(left: hasInset ? NavigationCompactTitleInset : 0, 0, 0, 0);
	}

	/// <summary>
	/// Compute the title bar drag regions for the current window size.
	/// </summary>
	public TitleBarDragRegions ComputeTitleBarDragRegions() => new(
		LeftStart: NavigationCompactWidth,
		LeftWidth: TitleBarAreaLeftColumn.ActualWidth - NavigationCompactWidth + 8,
		RightStart: TitleBarAreaLeftColumn.ActualWidth + TitleBarAreaCenterColumn.ActualWidth - 8,
		RightWidth: TitleBarAreaRightColumn.ActualWidth + 8);
}
