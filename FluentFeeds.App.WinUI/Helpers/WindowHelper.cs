using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using WinRT.Interop;

namespace FluentFeeds.App.WinUI.Helpers;

public static class WindowHelper
{
	public static AppWindow GetAppWindow(this Window window)
	{
		var windowHandle = WindowNative.GetWindowHandle(window);
		var windowId = Win32Interop.GetWindowIdFromWindow(windowHandle);
		return AppWindow.GetFromWindowId(windowId);
	}
}
