using System;
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

		DataContextChanged += HandleDataContextChanged;
	}

	public NavigationItemViewModel? ViewModel { get; private set; }

	private void UpdateActions()
	{
		var actions = ViewModel?.Actions ?? ImmutableArray<NavigationItemActionViewModel>.Empty;
		if (actions.Length != 0)
		{
			_itemStyle ??= Application.Current.Resources["DefaultMenuFlyoutItemStyle"] as Style;
			_menuFlyout ??= new MenuFlyout();
			_menuFlyout.Items.Clear();
			foreach (var action in actions)
			{
				_menuFlyout.Items.Add(new NavigationItemActionView { DataContext = action, Style = _itemStyle });
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
			UpdateActions();
		}
	}

	private void HandleDataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
	{
		if (!Object.ReferenceEquals(DataContext, ViewModel))
		{
			if (ViewModel != null)
				ViewModel.PropertyChanged -= HandlePropertyChanged;
			ViewModel = DataContext as NavigationItemViewModel;
			if (ViewModel != null)
				ViewModel.PropertyChanged += HandlePropertyChanged;
			UpdateActions();
		}
	}

	private MenuFlyout? _menuFlyout;
	private Style? _itemStyle;
}
