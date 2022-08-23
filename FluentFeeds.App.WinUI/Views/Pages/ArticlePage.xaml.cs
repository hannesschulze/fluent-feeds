using FluentFeeds.App.Shared.Models.Navigation;
using System;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using FluentFeeds.App.Shared.ViewModels.Pages;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using FluentFeeds.Feeds.Base.Items.Content;

namespace FluentFeeds.App.WinUI.Views.Pages;

/// <summary>
/// Page displaying an article.
/// </summary>
public sealed partial class ArticlePage : Page
{
	public ArticlePage()
	{
		DataContext = Ioc.Default.GetRequiredService<ArticleViewModel>();
		InitializeComponent();
	}

	public ArticleViewModel ViewModel => (ArticleViewModel)DataContext;

	protected override void OnNavigatedTo(NavigationEventArgs e)
	{
		base.OnNavigatedTo(e);
		MainScrollViewer.ScrollToVerticalOffset(0);
		ViewModel.Load((FeedNavigationRoute)e.Parameter);
	}
}
