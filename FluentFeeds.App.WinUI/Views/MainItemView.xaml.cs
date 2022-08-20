using System.Collections.Immutable;
using System.ComponentModel;
using FluentFeeds.App.Shared.ViewModels.Main;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace FluentFeeds.App.WinUI.Views;

/// <summary>
/// <para>A navigation view item which presents a <see cref="MainItemViewModel"/>.</para>
/// 
/// <para>This subclass is currently necessary because we need to keep track of the menu flyout items manually (because
/// MenuFlyout does not support ItemsSource and ItemTemplate).</para>
/// </summary>
public sealed partial class MainItemView : NavigationViewItem
{
	public MainItemView()
	{
		InitializeComponent();

		Loading += HandleLoading;
	}

	public MainItemViewModel? ViewModel => (MainItemViewModel?)DataContext;

	private void UpdateActions(ImmutableArray<MainItemActionViewModel>? actions)
	{
		if (actions.HasValue)
		{
			_menuFlyout ??= new MenuFlyout();
			_menuFlyout.Items.Clear();
			foreach (var action in actions.Value)
			{
				_menuFlyout.Items.Add(new MainItemActionView { DataContext = action });
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
		if (ViewModel != null && e.PropertyName == nameof(MainItemViewModel.Actions))
		{
			UpdateActions(ViewModel.Actions);
		}
	}

	private void HandleLoading(FrameworkElement sender, object args)
	{
		if (ViewModel != null)
		{
			ViewModel.PropertyChanged += HandlePropertyChanged;
			UpdateActions(ViewModel.Actions);
		}
	}

	private MenuFlyout? _menuFlyout;
}
