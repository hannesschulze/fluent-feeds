using System.Collections.Immutable;
using System.ComponentModel;
using System.Linq;
using FluentFeeds.App.Shared.Models;
using FluentFeeds.App.Shared.ViewModels.Pages;
using FluentFeeds.App.WinUI.Helpers;
using FluentFeeds.Feeds.Base.Items;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;

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
		SelectSortModeCommand = new RelayCommand<ItemSortMode>(sortMode => ViewModel.SelectedSortMode = sortMode);

		UpdateSelectedItems();
		ViewModel.PropertyChanged += HandlePropertyChanged;
	}

	public FeedViewModel ViewModel => (FeedViewModel)DataContext;

	private IconElement SynchronizeSymbol { get; }
	private IconElement RefreshSymbol { get; }
	private IconElement TrashSymbol { get; }
	private IconElement SortOrderSymbol { get; }
	private IconElement OpenExternalSymbol { get; }
	private IconElement FontSymbol { get; }

	private RelayCommand<ItemSortMode> SelectSortModeCommand { get; }

	private void HandlePropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		switch (e.PropertyName)
		{
			case nameof(FeedViewModel.SelectedItems):
				UpdateSelectedItems();
				break;
		}
	}

	private void HandleSelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (_isChangingSelection)
			return;

		_isChangingSelection = true;
		ViewModel.SelectedItems = MainItemList.SelectedItems
			.Select(item => (IReadOnlyStoredItem)item)
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

	private bool IsSortModeSelected(ItemSortMode sortMode, ItemSortMode itemMode) =>
		sortMode == itemMode;

	private bool _isChangingSelection;
}
