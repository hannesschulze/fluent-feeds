using System.IO;
using FluentFeeds.Shared.ViewModels;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Windows.ApplicationModel;

namespace FluentFeeds.WinUI.Pages;

public sealed partial class MainWindow : Window
{
	public MainWindow(MainViewModel viewModel)
	{
		ViewModel = viewModel;
		InitializeComponent();

		var windowHandle = WinRT.Interop.WindowNative.GetWindowHandle(this);
		var windowId = Win32Interop.GetWindowIdFromWindow(windowHandle);
		var appWindow = AppWindow.GetFromWindowId(windowId);
		var iconPath = Path.Combine(Package.Current.InstalledLocation.Path, "Assets", "WindowIcon.ico");
		appWindow.SetIcon(iconPath);
	}

	public MainViewModel ViewModel { get; }
}
