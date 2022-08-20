using System.Collections.Immutable;
using System.ComponentModel;
using FluentFeeds.App.Shared.ViewModels.Items.Navigation;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace FluentFeeds.App.WinUI.Views.Items.Navigation;

/// <summary>
/// <para>A navigation view item which presents a <see cref="NavigationItemViewModel"/>.</para>
/// 
/// <para>This subclass is currently necessary because we need to keep track of the menu flyout items manually (because
/// MenuFlyout does not support ItemsSource and ItemTemplate).</para>
/// </summary>
public sealed partial class NavigationItemView : NavigationViewItem
{
	public NavigationItemView()
	{
		InitializeComponent();

		Loading += HandleLoading;
	}

	public NavigationItemViewModel ViewModel => (NavigationItemViewModel)DataContext;

	private void UpdateActions(ImmutableArray<NavigationItemActionViewModel>? actions)
	{
		if (actions.HasValue)
		{
			_menuFlyout ??= new MenuFlyout();
			_menuFlyout.Items.Clear();
			foreach (var action in actions.Value)
			{
				_menuFlyout.Items.Add(new NavigationItemActionView { DataContext = action });
			}
			ContextFlyout = _menuFlyout;
		}
		else
		{
			ContextFlyout = null;
		}
	}

	private void HandlePropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName == nameof(NavigationItemViewModel.Actions))
		{
			UpdateActions(ViewModel.Actions);
		}
	}

	private void HandleLoading(FrameworkElement sender, object args)
	{
		ViewModel.PropertyChanged += HandlePropertyChanged;
		UpdateActions(ViewModel.Actions);
	}

	private MenuFlyout? _menuFlyout;
}
