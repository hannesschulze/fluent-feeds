using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using AngleSharp.Dom;
using FluentFeeds.App.Shared.Models;
using FluentFeeds.App.Shared.Services;
using FluentFeeds.Common;
using FluentFeeds.Feeds.Base;
using FluentFeeds.Feeds.Base.EventArgs;
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
	public FeedViewModel(IModalService modalService, IFeedService feedService)
	{
		_modalService = modalService;
		_feedService = feedService;
		
		_syncCommand = new RelayCommand(HandleSyncCommand, () => !IsSyncInProgress);
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
		if (_feed != null)
		{
			_feed.ItemsUpdated -= HandleItemsUpdated;
		}

		_feed = node.Feed;
		_syncToken = _updateItemsToken = null;
		IsSyncInProgress = false;

		UpdateItems(reload: true);
		_feed.ItemsUpdated += HandleItemsUpdated;

		if (_feed.LastSynchronized == null)
		{
			// This is the first time the feed is shown, it still needs to be synchronized.
			HandleSyncCommand();
		}
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
		set
		{
			if (SetProperty(ref _selectedSortMode, value))
			{
				UpdateItems(reload: true);
			}
		}
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

	private static IEnumerable<IReadOnlyStoredItem> SortItems(
		IEnumerable<IReadOnlyStoredItem> items, ItemSortMode sortMode)
	{
		return
			sortMode switch
			{
				ItemSortMode.Newest => items.OrderByDescending(item => item.PublishedTimestamp),
				ItemSortMode.Oldest => items.OrderBy(item => item.PublishedTimestamp),
				_ => items
			};
	}

	private static bool ShouldInsertSortedItem(
		IReadOnlyStoredItem existing, IReadOnlyStoredItem added, ItemSortMode sortMode)
	{
		return
			sortMode switch
			{
				ItemSortMode.Newest => existing.PublishedTimestamp < added.PublishedTimestamp,
				ItemSortMode.Oldest => existing.PublishedTimestamp > added.PublishedTimestamp,
				_ => false
			};
	}

	/// <summary>
	/// Reload the item list either completely or calculate differences between the previous and the new set of items.
	/// </summary>
	private async void UpdateItems(bool reload = false)
	{
		if (_feed == null)
			return;

		var token = new object();
		_updateItemsToken = token;
		var newItems = _feed.Items;
		var sortMode = SelectedSortMode;

		if (reload || _items.Count == 0)
		{
			_items.Clear();
			_itemSet = ImmutableHashSet<IReadOnlyStoredItem>.Empty;
			SelectedItems = ImmutableArray<IReadOnlyStoredItem>.Empty;

			// Sort the items on a background thread. This is only possible because the PublishedTimestamp does not change.
			// When other sort modes are added, this will need to be done differently.
			var sortedItems = await Task.Run(() => SortItems(newItems, sortMode).ToList());

			if (_updateItemsToken == token)
			{
				foreach (var item in sortedItems)
				{
					_items.Add(item);
				}
				_itemSet = newItems;
			}
		}
		else
		{
			// Calculate the differences and sort new items on a background thread (see remarks above).
			var added = new List<IReadOnlyStoredItem>();
			var removed = ImmutableHashSet<IReadOnlyStoredItem>.Empty;
			var oldItems = _itemSet;
			await Task.Run(
				() =>
				{
					added = SortItems(newItems.Except(oldItems), sortMode).ToList();
					removed = oldItems.Except(newItems);
				});

			if (_updateItemsToken == token)
			{
				// Apply removed items.
				if (removed.Count != 0)
				{
					for (var i = _items.Count - 1; i >= 0; --i)
					{
						if (removed.Contains(_items[i]))
						{
							_items.RemoveAt(i);
						}
					}
				}

				// Apply added items.
				if (added.Count != 0)
				{
					var addedIndex = 0;
					for (var i = 0; i < _items.Count; ++i)
					{
						if (ShouldInsertSortedItem(_items[i], added[addedIndex], sortMode))
						{
							_items.Insert(i, added[addedIndex]);
							if (++addedIndex >= added.Count)
							{
								break;
							}
						}
					}
					// Add remaining items to the end of the list.
					for (; addedIndex < added.Count; ++addedIndex)
					{
						_items.Add(added[addedIndex]);
					}
				}

				_itemSet = newItems;
			}
		}
	}

	private void HandleItemsUpdated(object? sender, FeedItemsUpdatedEventArgs e)
	{
		UpdateItems();
	}

	private async void HandleSyncCommand()
	{
		if (_feed == null)
			return;

		var token = new object();
		_syncToken = token;
		IsSyncInProgress = true;

		try
		{
			await _feedService.InitializeAsync();
			try
			{
				await _feed.SynchronizeAsync();
			}
			catch (Exception)
			{
				// TODO: Show error
			}
		}
		catch (Exception)
		{
			// Handled by main view model
		}

		if (_syncToken == token)
		{
			IsSyncInProgress = false;
		}
	}

	private readonly IModalService _modalService;
	private readonly IFeedService _feedService;
	private readonly RelayCommand _syncCommand;
	private readonly RelayCommand _toggleReadCommand;
	private readonly RelayCommand _deleteCommand;
	private readonly RelayCommand _openDisplayOptionsCommand;
	private readonly RelayCommand _reloadContentCommand;
	private readonly RelayCommand _openBrowserCommand;
	private readonly ObservableCollection<IReadOnlyStoredItem> _items = new();
	private ImmutableHashSet<IReadOnlyStoredItem> _itemSet = ImmutableHashSet<IReadOnlyStoredItem>.Empty;
	private object? _syncToken;
	private object? _updateItemsToken;
	private Feed? _feed;
	private ImmutableArray<IReadOnlyStoredItem> _selectedItems = ImmutableArray<IReadOnlyStoredItem>.Empty;
	private ItemSortMode _selectedSortMode = ItemSortMode.Newest;
	private Symbol _toggleReadSymbol = Symbol.MailUnread;
	private bool _isSyncInProgress;
	private bool _isItemSelected;
	private bool _isReloadContentAvailable;
}
