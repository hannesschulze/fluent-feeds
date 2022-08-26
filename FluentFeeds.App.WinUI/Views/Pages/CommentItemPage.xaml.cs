using FluentFeeds.App.Shared.Models.Navigation;
using FluentFeeds.App.Shared.ViewModels.Pages;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace FluentFeeds.App.WinUI.Views.Pages;

/// <summary>
/// Page displaying a content item.
/// </summary>
public sealed partial class CommentItemPage : Page
{
	public CommentItemPage()
	{
		DataContext = Ioc.Default.GetRequiredService<CommentItemViewModel>();
		InitializeComponent();
	}

	public CommentItemViewModel ViewModel => (CommentItemViewModel)DataContext;

	protected override void OnNavigatedTo(NavigationEventArgs e)
	{
		base.OnNavigatedTo(e);
		ViewModel.Load((FeedNavigationRoute)e.Parameter);
	}
}
