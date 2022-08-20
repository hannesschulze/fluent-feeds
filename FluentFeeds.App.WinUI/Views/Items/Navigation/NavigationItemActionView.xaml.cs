using FluentFeeds.App.Shared.ViewModels.Items.Navigation;
using Microsoft.UI.Xaml.Controls;

namespace FluentFeeds.App.WinUI.Views.Items.Navigation;

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
