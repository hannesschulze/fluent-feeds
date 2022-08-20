using FluentFeeds.App.Shared.ViewModels.Main;
using Microsoft.UI.Xaml.Controls;

namespace FluentFeeds.App.WinUI.Views;

/// <summary>
/// A menu flyout item which presents a <see cref="MainItemActionViewModel"/>.
/// </summary>
public sealed partial class MainItemActionView : MenuFlyoutItem
{
	public MainItemActionView()
	{
		InitializeComponent();
	}

	public MainItemActionViewModel? ViewModel => (MainItemActionViewModel?)DataContext;
}
