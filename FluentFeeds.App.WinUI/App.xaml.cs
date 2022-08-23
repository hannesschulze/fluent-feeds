using FluentFeeds.App.Shared.Services;
using FluentFeeds.App.Shared.Services.Default;
using FluentFeeds.App.Shared.ViewModels.Pages;
using FluentFeeds.App.WinUI.Services;
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
				.AddSingleton<IPluginService, PluginService>()
				.AddSingleton<IDatabaseService, DatabaseService>()
				.AddSingleton<IFeedService, FeedService>()
				.AddSingleton<IModalService, ModalService>()
				.AddTransient<MainViewModel>()
				.AddTransient<FeedViewModel>()
				.BuildServiceProvider());

		_window = new MainWindow();
		_window.Activate();
	}

	private Window? _window;
}
