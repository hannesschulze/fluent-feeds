using FluentFeeds.App.Shared.ViewModels.ListItems.Navigation;
using Microsoft.UI.Xaml.Controls;

namespace FluentFeeds.App.WinUI.Views.ListItems.Navigation;

/// <summary>
/// A menu flyout item which presents a <see cref="NavigationItemActionViewModel"/>.
/// </summary>
public sealed partial class NavigationItemActionView : MenuFlyoutItem
{
	public NavigationItemActionView()
	{
		InitializeComponent();
	}

	public NavigationItemActionViewModel ViewModel => (NavigationItemActionViewModel)DataContext;
}
