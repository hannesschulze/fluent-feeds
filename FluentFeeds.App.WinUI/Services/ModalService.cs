using System;
using FluentFeeds.App.Shared.Services;
using FluentFeeds.App.Shared.ViewModels.Items.Navigation;
using FluentFeeds.App.Shared.ViewModels.Modals;
using FluentFeeds.App.WinUI.Views.Modals;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace FluentFeeds.App.WinUI.Services;

public sealed class ModalService : IModalService
{
	public Func<XamlRoot?>? XamlRootLocator { get; set; }

	public Func<NavigationItemViewModel, FrameworkElement?>? NavigationItemLocator { get; set; }

	public Func<InfoBar?>? ErrorBarLocator { get; set; }

	private async void ShowDialog(ContentDialog dialog)
	{
		if (XamlRootLocator == null)
			throw new Exception("Attempting to show a content dialog without a XAML root locator.");
		var xamlRoot = XamlRootLocator.Invoke();
		if (xamlRoot != null)
		{
			dialog.XamlRoot = xamlRoot;
			dialog.Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style;
			await dialog.ShowAsync();
		}
	}

	private void ShowNavigationFlyout(FlyoutView flyout, NavigationItemViewModel itemViewModel)
	{
		if (NavigationItemLocator == null)
			throw new Exception("Attempting to show a flyout for a navigation item without an item locator.");
		var item = NavigationItemLocator.Invoke(itemViewModel);
		if (item != null)
		{
			flyout.CreateFlyout().ShowAt(item);
		}
	}

	public void Show(FeedDataViewModel viewModel, NavigationItemViewModel relatedItem)
	{
		ShowDialog(new FeedDataView { DataContext = viewModel });
	}

	public void Show(DeleteFeedViewModel viewModel, NavigationItemViewModel relatedItem)
	{
		ShowNavigationFlyout(new DeleteFeedView { DataContext = viewModel }, relatedItem);
	}

	public void Show(ErrorViewModel viewModel)
	{
		if (ErrorBarLocator == null)
			throw new Exception("Attempting to show an error without an error bar locator.");
		var errorBar = ErrorBarLocator.Invoke();
		if (errorBar != null)
		{
			errorBar.Title = viewModel.Title;
			errorBar.Message = viewModel.Message;
			errorBar.IsOpen = true;
			errorBar.Margin = new Thickness(0, 0, 0, bottom: 8);
		}
	}
}
