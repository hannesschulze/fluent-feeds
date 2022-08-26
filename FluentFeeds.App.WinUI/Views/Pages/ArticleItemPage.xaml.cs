using FluentFeeds.App.Shared.Models.Navigation;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using FluentFeeds.App.Shared.ViewModels.Pages;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using Microsoft.UI.Xaml.Media;
using System;

namespace FluentFeeds.App.WinUI.Views.Pages;

/// <summary>
/// Page displaying an article.
/// </summary>
public sealed partial class ArticleItemPage : Page
{
	public ArticleItemPage()
	{
		DataContext = Ioc.Default.GetRequiredService<ArticleItemViewModel>();
		InitializeComponent();
	}

	public ArticleItemViewModel ViewModel => (ArticleItemViewModel)DataContext;

	protected override void OnNavigatedTo(NavigationEventArgs e)
	{
		base.OnNavigatedTo(e);
		MainScrollViewer.ScrollToVerticalOffset(0);
		ViewModel.Load((FeedNavigationRoute)e.Parameter);
	}
}
