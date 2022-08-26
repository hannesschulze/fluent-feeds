using System;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Linq;
using FluentFeeds.App.Shared.Models;
using FluentFeeds.App.Shared.Models.Items;
using FluentFeeds.App.Shared.Models.Navigation;
using FluentFeeds.App.Shared.ViewModels.Pages;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;

namespace FluentFeeds.App.WinUI.Views.Pages;

public sealed partial class FeedPage : Page
{
	public FeedPage()
	{
		DataContext = Ioc.Default.GetRequiredService<FeedViewModel>();
		InitializeComponent();

		SelectSortModeCommand = new RelayCommand<ItemSortMode>(sortMode => ViewModel.SelectedSortMode = sortMode);
		SelectFontFamilyCommand = new RelayCommand<FontFamily>(
			fontFamily => ViewModel.DisplayOptions.SelectedFontFamily = fontFamily);

		UpdateSelectedItems();
		UpdateCurrentRoute();
		ViewModel.PropertyChanged += HandlePropertyChanged;
	}

	public FeedViewModel ViewModel => (FeedViewModel)DataContext;

	private RelayCommand<ItemSortMode> SelectSortModeCommand { get; }
	private RelayCommand<FontFamily> SelectFontFamilyCommand { get; }

	protected override void OnNavigatedTo(NavigationEventArgs e)
	{
		base.OnNavigatedTo(e);
		ViewModel.Load((MainNavigationRoute)e.Parameter);
	}

	private void HandlePropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		switch (e.PropertyName)
		{
			case nameof(FeedViewModel.SelectedItems):
				UpdateSelectedItems();
				break;
			case nameof(FeedViewModel.CurrentRoute):
				UpdateCurrentRoute();
				break;
		}
	}

	private void HandleSelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (_isChangingSelection)
			return;

		_isChangingSelection = true;
		ViewModel.SelectedItems = MainItemList.SelectedItems
			.Select(item => (IItemView)item)
			.ToImmutableArray();
		_isChangingSelection = false;
	}

	private void UpdateSelectedItems()
	{
		if (_isChangingSelection)
			return;

		_isChangingSelection = true;
		MainItemList.SelectedItems.Clear();
		foreach (var item in ViewModel.SelectedItems)
			MainItemList.SelectedItems.Add(item);
		_isChangingSelection = false;
	}

	private void UpdateCurrentRoute() =>
		MainContentFrame.Navigate(
			ViewModel.CurrentRoute.Type switch
			{
				FeedNavigationRouteType.Article => typeof(ArticlePage),
				FeedNavigationRouteType.Selection => typeof(SelectionPage),
				_ => throw new IndexOutOfRangeException()
			}, ViewModel.CurrentRoute, new EntranceNavigationTransitionInfo());

	private bool IsSortModeSelected(ItemSortMode sortMode, ItemSortMode itemMode) =>
		sortMode == itemMode;

	private bool IsFontFamilySelected(FontFamily fontFamily, FontFamily itemFontFamily) =>
		fontFamily == itemFontFamily;

	private bool _isChangingSelection;
}
