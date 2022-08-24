using FluentFeeds.App.Shared.Models.Navigation;
using FluentFeeds.App.Shared.ViewModels.Pages;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace FluentFeeds.App.WinUI.Views.Pages;

/// <summary>
/// Page showing information about the currently selected items.
/// </summary>
public sealed partial class SelectionPage : Page
{
	public SelectionPage()
	{
		DataContext = Ioc.Default.GetRequiredService<SelectionViewModel>();
		InitializeComponent();
	}

	public SelectionViewModel ViewModel => (SelectionViewModel)DataContext;

	protected override void OnNavigatedTo(NavigationEventArgs e)
	{
		base.OnNavigatedTo(e);
		ViewModel.Load((FeedNavigationRoute)e.Parameter);
	}
}
