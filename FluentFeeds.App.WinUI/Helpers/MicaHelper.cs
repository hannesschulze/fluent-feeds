using System;
using System.Runtime.InteropServices;
using Microsoft.UI.Composition;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Xaml;
using Windows.System;
using WinRT;

namespace FluentFeeds.App.WinUI.Helpers;

/// <summary>
/// Helper class which applies a Mica backdrop to a window.
/// </summary>
public class MicaHelper
{
	[StructLayout(LayoutKind.Sequential)]
	private struct DispatcherQueueOptions
	{
		internal int dwSize;
		internal int threadType;
		internal int apartmentType;
	}

	[DllImport("CoreMessaging.dll")]
	private static extern int CreateDispatcherQueueController(
		[In] DispatcherQueueOptions options,
		[In, Out, MarshalAs(UnmanagedType.IUnknown)] ref object? dispatcherQueueController);

	/// <summary>
	/// Initialize the Mica backdrop behind a window.
	/// </summary>
	public MicaHelper(Window window)
	{
		_dispatcherQueueController = EnsureDispatcherQueue();

		_configuration = new SystemBackdropConfiguration();
		_configuration.IsInputActive = true;
		if (window.Content is FrameworkElement content)
		{
			UpdateBackdropConfiguration(content);
			content.ActualThemeChanged += (s, e) => UpdateBackdropConfiguration(s);
		}

		_micaController = new MicaController();
		_micaController.AddSystemBackdropTarget(window.As<ICompositionSupportsSystemBackdrop>());
		_micaController.SetSystemBackdropConfiguration(_configuration);

		window.Activated += HandleWindowActivated;
		window.Closed += HandleWindowClosed;

		GC.KeepAlive(_dispatcherQueueController);
	}

	private static object? EnsureDispatcherQueue()
	{
		if (DispatcherQueue.GetForCurrentThread() != null)
			// One already exists, so we'll just use it.
			return null;

		DispatcherQueueOptions options;
		options.dwSize = Marshal.SizeOf(typeof(DispatcherQueueOptions));
		options.threadType = 2;    // DQTYPE_THREAD_CURRENT
		options.apartmentType = 2; // DQTAT_COM_STA

		object? controller = null;
		if (CreateDispatcherQueueController(options, ref controller) != 0)
			throw new Exception("Unable to create dispatcher queue controller.");

		return controller;
	}

	private void HandleWindowActivated(object sender, WindowActivatedEventArgs e)
	{
		if (_configuration == null)
			return;

		_configuration.IsInputActive = e.WindowActivationState != WindowActivationState.Deactivated;
	}

	private void HandleWindowClosed(object sender, WindowEventArgs e)
	{
		_micaController?.Dispose();
		_micaController = null;
		_configuration = null;
	}

	private void UpdateBackdropConfiguration(FrameworkElement content)
	{
		if (_configuration == null)
			return;

		_configuration.Theme =
			content.ActualTheme switch
			{
				ElementTheme.Light => SystemBackdropTheme.Light,
				ElementTheme.Dark => SystemBackdropTheme.Dark,
				_ => SystemBackdropTheme.Default,
			};
	}

	private readonly object? _dispatcherQueueController;
	private SystemBackdropConfiguration? _configuration;
	private MicaController? _micaController;
}
