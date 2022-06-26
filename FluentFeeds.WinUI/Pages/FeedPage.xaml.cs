using FluentFeeds.Shared.Services;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace FluentFeeds.WinUI.Pages;

public sealed partial class FeedPage : Page
{
	public FeedPage()
	{
		InitializeComponent();
		var navigationService = Ioc.Default.GetRequiredService<INavigationService>();
		var updateLabel = () => _lbl.Text = navigationService.CurrentRoute.FeedSource?.Name ?? "Unknown feed";
		navigationService.BackStackChanged += (s, e) => updateLabel();
		updateLabel();
	}
}
