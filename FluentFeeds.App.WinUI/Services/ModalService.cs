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

	public void ShowModal(NodeDataViewModel viewModel, NavigationItemViewModel relatedItem) =>
		ShowDialog(new NodeDataView { DataContext = viewModel });

	public void ShowModal(DeleteNodeViewModel viewModel, NavigationItemViewModel relatedItem) =>
		ShowNavigationFlyout(new DeleteNodeView { DataContext = viewModel }, relatedItem);
}
