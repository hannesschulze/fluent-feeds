using FluentFeeds.App.Shared.Services;
using FluentFeeds.App.Shared.Services.Default;
using FluentFeeds.App.Shared.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using Microsoft.UI.Xaml;

namespace FluentFeeds.App.WinUI;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();
	}

	protected override void OnLaunched(LaunchActivatedEventArgs args)
	{
		Ioc.Default.ConfigureServices(
			new ServiceCollection()
				.AddSingleton<INavigationService, DefaultNavigationService>()
				.AddTransient<MainViewModel>()
				.BuildServiceProvider());

		_window = new MainWindow();
		_window.Activate();
	}

	private Window? _window;
}
