using System;
using System.Diagnostics;
using FluentFeeds.App.Shared.Services;
using FluentFeeds.App.Shared.Services.Default;
using FluentFeeds.App.Shared.ViewModels.Pages;
using FluentFeeds.App.WinUI.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.Windows.AppLifecycle;
using WinRT.Interop;

namespace FluentFeeds.App.WinUI;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();
	}

	protected override async void OnLaunched(LaunchActivatedEventArgs args)
	{
		// Singal app instance based on: https://gist.github.com/andrewleader/5adc742fe15b06576c1973ea6e999552
		var appInstance = AppInstance.GetCurrent();
		var mainInstance = AppInstance.FindOrRegisterForKey("main");
		if (!mainInstance.IsCurrent)
		{
			// Redirect to the main instance and kill the process.
			var activationArgs = appInstance.GetActivatedEventArgs();
			await mainInstance.RedirectActivationToAsync(activationArgs);
			Process.GetCurrentProcess().Kill();
		}

		// This is the main instance, initialize services and open the main window.
		Ioc.Default.ConfigureServices(
			new ServiceCollection()
				.AddSingleton<IDatabaseService, DatabaseService>()
				.AddSingleton<IFeedService, FeedService>()
				.AddSingleton<ISettingsService, SettingsService>()
				.AddSingleton<IWebBrowserService, WebBrowserService>()
				.AddSingleton<IModalService, ModalService>()
				.AddTransient<MainViewModel>()
				.AddTransient<FeedViewModel>()
				.AddTransient<SelectionViewModel>()
				.AddTransient<ArticleItemViewModel>()
				.AddTransient<CommentItemViewModel>()
				.AddTransient<SettingsViewModel>()
				.BuildServiceProvider());

		_window = new MainWindow();
		_window.Activate();
		appInstance.Activated += HandleAppInstanceActivated;
	}

	private void HandleAppInstanceActivated(object? sender, AppActivationArguments e)
	{
		if (_window != null)
		{
			var hWnd = (Windows.Win32.Foundation.HWND)WindowNative.GetWindowHandle(_window);
			Windows.Win32.PInvoke.ShowWindow(hWnd, Windows.Win32.UI.WindowsAndMessaging.SHOW_WINDOW_CMD.SW_RESTORE);
			Windows.Win32.PInvoke.SetForegroundWindow(hWnd);
		}
	}

	private Window? _window;
}
