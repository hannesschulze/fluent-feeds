using System;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Linq;
using FluentFeeds.App.Shared.Models;
using FluentFeeds.App.Shared.Models.Items;
using FluentFeeds.App.Shared.Models.Navigation;
using FluentFeeds.App.Shared.ViewModels.Pages;
using FluentFeeds.App.WinUI.Helpers;
using FluentFeeds.Feeds.Base.Items;
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

		SynchronizeSymbol = Common.Symbol.Synchronize.ToIconElement();
		RefreshSymbol = Common.Symbol.Refresh.ToIconElement();
		TrashSymbol = Common.Symbol.Trash.ToIconElement();
		SortOrderSymbol = Common.Symbol.SortOrder.ToIconElement();
		OpenExternalSymbol = Common.Symbol.OpenExternal.ToIconElement();
		FontSymbol = Common.Symbol.Font.ToIconElement();
		FontFamilySymbol = Common.Symbol.FontFamily.ToIconElement();
		FontSizeIncreaseSymbol = Common.Symbol.FontSizeIncrease.ToIconElement();
		FontSizeDecreaseSymbol = Common.Symbol.FontSizeDecrease.ToIconElement();
		FontSizeResetSymbol = Common.Symbol.FontSizeReset.ToIconElement();
		SelectSortModeCommand = new RelayCommand<ItemSortMode>(sortMode => ViewModel.SelectedSortMode = sortMode);
		SelectFontFamilyCommand = new RelayCommand<FontFamily>(
			fontFamily => ViewModel.DisplayOptions.SelectedFontFamily = fontFamily);

		UpdateSelectedItems();
		UpdateCurrentRoute();
		ViewModel.PropertyChanged += HandlePropertyChanged;
	}

	public FeedViewModel ViewModel => (FeedViewModel)DataContext;

	private IconElement SynchronizeSymbol { get; }
	private IconElement RefreshSymbol { get; }
	private IconElement TrashSymbol { get; }
	private IconElement SortOrderSymbol { get; }
	private IconElement OpenExternalSymbol { get; }
	private IconElement FontSymbol { get; }
	private IconElement FontFamilySymbol { get; }
	private IconElement FontSizeIncreaseSymbol { get; }
	private IconElement FontSizeDecreaseSymbol { get; }
	private IconElement FontSizeResetSymbol { get; }

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
