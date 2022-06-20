using FluentFeeds.Shared.ViewModels;
using FluentFeeds.WinUI.Pages;
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
		_window = new MainWindow(new MainViewModel());
		_window.Activate();
	}

	private Window? _window;
}
