using FluentFeeds.Shared.Services;
using FluentFeeds.Shared.Services.Default;
using FluentFeeds.Shared.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using Microsoft.UI.Xaml;

namespace FluentFeeds.WinUI;

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
