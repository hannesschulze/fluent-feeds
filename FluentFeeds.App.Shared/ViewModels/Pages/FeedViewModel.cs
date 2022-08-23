using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Windows.Input;
using FluentFeeds.App.Shared.Models;
using FluentFeeds.App.Shared.Services;
using FluentFeeds.Common;
using FluentFeeds.Feeds.Base.Items;
using FluentFeeds.Feeds.Base.Nodes;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;

namespace FluentFeeds.App.Shared.ViewModels.Pages;

/// <summary>
/// View model displaying the currently selected feed. The feed view contains both the item list and the content view
/// and manages navigation for them. It also contains the toolbar.
/// </summary>
public sealed class FeedViewModel : ObservableObject
{
	public FeedViewModel(IModalService modalService)
	{
		_modalService = modalService;
		
		_syncCommand = new RelayCommand(() => { }, () => !IsSyncInProgress);
		_toggleReadCommand = new RelayCommand(() => { }, () => IsItemSelected);
		_deleteCommand = new RelayCommand(() => { }, () => IsItemSelected);
		_openDisplayOptionsCommand = new RelayCommand(() => { });
		_reloadContentCommand = new RelayCommand(() => { }, () => IsReloadContentAvailable);
		_openBrowserCommand = new RelayCommand(() => { });

		Items = new ReadOnlyObservableCollection<IReadOnlyStoredItem>(_items);
	}
	
	/// <summary>
	/// Called after navigating to the feed view.
	/// </summary>
	/// <param name="node">The node of the current <see cref="NavigationRoute"/>.</param>
	public void Load(IReadOnlyFeedNode node)
	{
	}

	/// <summary>
	/// Synchronize the currently selected feed, fetching new items from the web.
	/// </summary>
	public ICommand SyncCommand => _syncCommand;

	/// <summary>
	/// Toggle the currently selected item's "read" state.
	/// </summary>
	public ICommand ToggleReadCommand => _toggleReadCommand;

	/// <summary>
	/// Delete the currently selected item(s).
	/// </summary>
	public ICommand DeleteCommand => _deleteCommand;

	/// <summary>
	/// Open display options for the reader.
	/// </summary>
	public ICommand OpenDisplayOptionsCommand => _openDisplayOptionsCommand;

	/// <summary>
	/// Reload the content of the currently selected item.
	/// </summary>
	public ICommand ReloadContentCommand => _reloadContentCommand;

	/// <summary>
	/// Open the content of the currently selected item in a web browser.
	/// </summary>
	public ICommand OpenBrowserCommand => _openBrowserCommand;
	
	/// <summary>
	/// Items provided by the feed, sorted using the current sort mode.
	/// </summary>
	public ReadOnlyObservableCollection<IReadOnlyStoredItem> Items { get; }

	/// <summary>
	/// List of currently selected items.
	/// </summary>
	public ImmutableArray<IReadOnlyStoredItem> SelectedItems
	{
		get => _selectedItems;
		set
		{
			if (SetProperty(ref _selectedItems, value))
			{
				IsItemSelected = SelectedItems.Length >= 1;
			}
		}
	}

	/// <summary>
	/// Mode used for sorting the items.
	/// </summary>
	public ItemSortMode SelectedSortMode
	{
		get => _selectedSortMode;
		set => SetProperty(ref _selectedSortMode, value);
	}

	/// <summary>
	/// Symbol for <see cref="ToggleReadCommand"/>.
	/// </summary>
	public Symbol ToggleReadSymbol
	{
		get => _toggleReadSymbol;
		private set => SetProperty(ref _toggleReadSymbol, value);
	}

	/// <summary>
	/// Indicates if the app is currently fetching new feeds from the web.
	/// </summary>
	public bool IsSyncInProgress
	{
		get => _isSyncInProgress;
		private set
		{
			if (SetProperty(ref _isSyncInProgress, value))
			{
				_syncCommand.NotifyCanExecuteChanged();
			}
		}
	}
	
	/// <summary>
	/// Flag indicating if an item is currently selected.
	/// </summary>
	/// <remarks>
	/// Based on this property, the visibility for the options to execute <see cref="ToggleReadCommand"/> and
	/// <see cref="DeleteCommand"/> should be updated.
	/// </remarks>
	public bool IsItemSelected
	{
		get => _isItemSelected;
		private set
		{
			if (SetProperty(ref _isItemSelected, value))
			{
				_toggleReadCommand.NotifyCanExecuteChanged();
				_deleteCommand.NotifyCanExecuteChanged();
			}
		}
	}

	/// <summary>
	/// Flag indicating if the option to execute <see cref="ReloadContentCommand"/> should be shown.
	/// </summary>
	/// <remarks>
	/// It does not make sense to reload static content. Because of this, the option is only shown if the content is
	/// dynamic.
	/// </remarks>
	public bool IsReloadContentAvailable
	{
		get => _isReloadContentAvailable;
		private set
		{
			if (SetProperty(ref _isReloadContentAvailable, value))
			{
				_reloadContentCommand.NotifyCanExecuteChanged();
			}
		}
	}

	private readonly IModalService _modalService;
	private readonly RelayCommand _syncCommand;
	private readonly RelayCommand _toggleReadCommand;
	private readonly RelayCommand _deleteCommand;
	private readonly RelayCommand _openDisplayOptionsCommand;
	private readonly RelayCommand _reloadContentCommand;
	private readonly RelayCommand _openBrowserCommand;
	private readonly ObservableCollection<IReadOnlyStoredItem> _items = new();
	private ImmutableArray<IReadOnlyStoredItem> _selectedItems = ImmutableArray<IReadOnlyStoredItem>.Empty;
	private ItemSortMode _selectedSortMode = ItemSortMode.Newest;
	private Symbol _toggleReadSymbol = Symbol.MailUnread;
	private bool _isSyncInProgress;
	private bool _isItemSelected;
	private bool _isReloadContentAvailable;
}
