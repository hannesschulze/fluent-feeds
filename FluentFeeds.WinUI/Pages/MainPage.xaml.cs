using System;
using System.IO;
using FluentFeeds.Shared.ViewModels;
using FluentFeeds.WinUI.Helpers;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.ApplicationModel;

namespace FluentFeeds.WinUI.Pages;

/// <summary>
/// The main page of the app managing the navigation view and the titlebar.
/// </summary>
public sealed partial class MainPage : Page
{
	private double SearchFieldMaxWidth => 500;
	private double NavigationExpandedWidth => 260;
	private double NavigationCompactWidth => 48;
	private double NavigationCompactTitleInset => 20;
	private GridLength SearchFieldMaxGridLength => new(SearchFieldMaxWidth);
	private GridLength NavigationCompactGridLength => new(NavigationCompactWidth);
	private GridLength NavigationExpandedGridLength => new(NavigationExpandedWidth);
	private GridLength CaptionButtonsGridLength => new(CaptionButtonsWidth);
	private double MinWidthForTitleVisible => NavigationCompactWidth + CaptionButtonsWidth + SearchFieldMaxWidth;
	private double MinWidthForSearchSpacing => NavigationExpandedWidth + CaptionButtonsWidth + SearchFieldMaxWidth;
	private double MinWidthForCenterSearch => NavigationExpandedWidth * 2 + SearchFieldMaxWidth;
	private Thickness TitleBarAreaLeftMargin => new(NavigationCompactWidth, 0, 0, 0);

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
		DataContext = new MainViewModel();
		InitializeComponent();

		_titleBar.Loaded += (s, e) => DragRegionSizeChanged?.Invoke(this, EventArgs.Empty);
		_navigationView.PaneClosing += (s, e) => UpdateTitleInset();
		_navigationView.PaneOpening += (s, e) => UpdateTitleInset();
		_navigationView.DisplayModeChanged += (s, e) => UpdateTitleInset();
		UpdateTitleInset();
	}

	private void HandleDragRegionSizeChanged(object sender, SizeChangedEventArgs e) => 
		DragRegionSizeChanged?.Invoke(this, EventArgs.Empty);

	/// <summary>
	/// Update the title's margin. Spacing is added before the title when the navigation bar is collapsed.
	/// </summary>
	private void UpdateTitleInset()
	{
		var hasInset = 
			_navigationView.DisplayMode != NavigationViewDisplayMode.Expanded || !_navigationView.IsPaneOpen;
		_titleBarTitle.Margin = new Thickness(
			left: hasInset ? NavigationCompactTitleInset : 0, top: 0, right: 0, bottom: 0);
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
