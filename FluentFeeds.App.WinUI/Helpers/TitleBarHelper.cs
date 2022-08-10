using System;
using System.Runtime.InteropServices;
using Microsoft.UI;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Windows.Graphics;
using Windows.UI;
using WinRT.Interop;

namespace FluentFeeds.App.WinUI.Helpers;

/// <summary>
/// Helper class which initializes a tall title bar for a window which extends into the window region and sets up 
/// draggable regions for interactive content.
/// </summary>
public class TitleBarHelper
{
	[DllImport("Shcore.dll", SetLastError = true)]
	private static extern int GetDpiForMonitor(IntPtr hmonitor, Monitor_DPI_Type dpiType, out uint dpiX, out uint dpiY);

	private enum Monitor_DPI_Type : int
	{
		MDT_Effective_DPI = 0,
		MDT_Angular_DPI = 1,
		MDT_Raw_DPI = 2,
		MDT_Default = MDT_Effective_DPI
	}

	public double CaptionButtonsWidth { get; }
	public double TitleBarHeight { get; }

	/// <summary>
	/// Initialize the title bar for a window.
	/// </summary>
	/// <param name="window">The window whose title bar should be initialized.</param>
	/// <param name="measureDragRegions">Function measuring the drag regions for the current window size.</param>
	public TitleBarHelper(Window window, Func<TitleBarDragRegions> measureDragRegions)
	{
		_window = window;
		_measureDragRegions = measureDragRegions;

		_titleBar = window.GetAppWindow().TitleBar;
		_titleBar.ExtendsContentIntoTitleBar = true;
		_titleBar.PreferredHeightOption = TitleBarHeightOption.Tall;
		_titleBar.IconShowOptions = IconShowOptions.HideIconAndSystemMenu;
		_titleBar.ButtonBackgroundColor = Color.FromArgb(0, 0, 0, 0);
		_titleBar.ButtonInactiveBackgroundColor = Color.FromArgb(0, 0, 0, 0);

		var scaleAdjustment = GetScaleAdjustment();
		if (scaleAdjustment < 0.00001)
			scaleAdjustment = 1;
		CaptionButtonsWidth = Math.Max(_titleBar.LeftInset, _titleBar.RightInset) / scaleAdjustment;
		TitleBarHeight = _titleBar.Height / scaleAdjustment;

		if (window.Content is FrameworkElement content)
		{
			content.ActualThemeChanged += (s, e) => UpdateThemeColors(content);
			UpdateThemeColors(content);
		}
	}

	/// <summary>
	/// Schedule an update to the drag regions. Updates are coalesced.
	/// </summary>
	public void UpdateDragRegions()
	{
		if (_updateRegionsScheduled)
			// Update is already scheduled.
			return;

		_updateRegionsScheduled = true;
		DispatcherQueue.GetForCurrentThread()
			.TryEnqueue(() =>
			{
				_updateRegionsScheduled = false;
				UpdateDragRegionsCore();
			});
	}

	private void UpdateDragRegionsCore()
	{
		var scaleAdjustment = GetScaleAdjustment();
		var dragRegions = _measureDragRegions();
		// XXX: Interactive content is not clickable after resize
		// https://github.com/microsoft/WindowsAppSDK/issues/2574
		_titleBar.SetDragRectangles(
			new[]
			{
				new RectInt32
				{
					X = (int)(dragRegions.LeftStart * scaleAdjustment),
					Y = 0,
					Width = (int)(dragRegions.LeftWidth * scaleAdjustment),
					Height = _titleBar.Height
				},
				new RectInt32
				{
					X = (int)(dragRegions.RightStart * scaleAdjustment),
					Y = 0,
					Width = (int)(dragRegions.RightWidth * scaleAdjustment),
					Height = _titleBar.Height
				}
			});
	}

	private double GetScaleAdjustment()
	{
		var windowHandle = WindowNative.GetWindowHandle(_window);
		var windowId = Win32Interop.GetWindowIdFromWindow(windowHandle);
		var displayArea = DisplayArea.GetFromWindowId(windowId, DisplayAreaFallback.Primary);
		var hMonitor = Win32Interop.GetMonitorFromDisplayId(displayArea.DisplayId);

		if (GetDpiForMonitor(hMonitor, Monitor_DPI_Type.MDT_Default, out uint dpiX, out uint _) != 0)
			throw new Exception("Could not get DPI for monitor.");

		uint scaleFactorPercent = (uint)(((long)dpiX * 100 + (96 >> 1)) / 96);
		return scaleFactorPercent / 100.0;
	}

	private void UpdateThemeColors(FrameworkElement content)
	{
		switch (content.ActualTheme)
		{
			case ElementTheme.Default: 
			case ElementTheme.Light:
				_titleBar.ButtonForegroundColor = Color.FromArgb(255, 0, 0, 0);
				_titleBar.ButtonHoverBackgroundColor = Color.FromArgb(20, 0, 0, 0);
				_titleBar.ButtonHoverForegroundColor = Color.FromArgb(255, 0, 0, 0);
				_titleBar.ButtonPressedBackgroundColor = Color.FromArgb(30, 0, 0, 0);
				_titleBar.ButtonPressedForegroundColor = Color.FromArgb(255, 0, 0, 0);
				break;
			case ElementTheme.Dark:
				_titleBar.ButtonForegroundColor = Color.FromArgb(255, 255, 255, 255);
				_titleBar.ButtonHoverBackgroundColor = Color.FromArgb(20, 255, 255, 255);
				_titleBar.ButtonHoverForegroundColor = Color.FromArgb(255, 255, 255, 255);
				_titleBar.ButtonPressedBackgroundColor = Color.FromArgb(40, 255, 255, 255);
				_titleBar.ButtonPressedForegroundColor = Color.FromArgb(255, 255, 255, 255);
				break;
		}
	}

	private readonly Window _window;
	private readonly Func<TitleBarDragRegions> _measureDragRegions;
	private readonly AppWindowTitleBar _titleBar;
	private bool _updateRegionsScheduled = false;
}
