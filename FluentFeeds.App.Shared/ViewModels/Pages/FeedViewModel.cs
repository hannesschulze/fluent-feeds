using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using FluentFeeds.App.Shared.EventArgs;
using FluentFeeds.App.Shared.Models;
using FluentFeeds.App.Shared.Models.Feeds;
using FluentFeeds.App.Shared.Models.Items;
using FluentFeeds.App.Shared.Models.Navigation;
using FluentFeeds.App.Shared.Models.Storage;
using FluentFeeds.App.Shared.Services;
using FluentFeeds.App.Shared.ViewModels.Modals;
using FluentFeeds.Common;
using FluentFeeds.Feeds.Base.Items.Content;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;

namespace FluentFeeds.App.Shared.ViewModels.Pages;

/// <summary>
/// View model displaying the currently selected feed. The feed view contains both the item list and the content view
/// and manages navigation for them. It also contains the toolbar.
/// </summary>
public sealed class FeedViewModel : ObservableObject
{
	private enum ToggleReadAction
	{
		MarkRead,
		MarkUnread
	}

	public event EventHandler? ItemsUpdated;

	public FeedViewModel(IModalService modalService, IFeedService feedService, ISettingsService settingsService)
	{
		_modalService = modalService;
		_feedService = feedService;

		_syncCommand = new RelayCommand(HandleSyncCommand, () => !IsSyncInProgress);
		_toggleReadCommand = new RelayCommand(HandleToggleReadCommand, () => IsItemSelected);
		_deleteCommand = new RelayCommand(HandleDeleteCommand, () => IsItemSelected);
		_reloadContentCommand = new RelayCommand(HandleReloadContentCommand, () => IsReloadContentAvailable);
		_openBrowserCommand = new RelayCommand(HandleOpenBrowserCommand, () => SelectedItems.Length == 1);

		Items = new ReadOnlyObservableCollection<IItemView>(_items);
		DisplayOptions = new DisplayOptionsViewModel(settingsService);
	}

	/// <summary>
	/// Called after navigating to the feed view.
	/// </summary>
	/// <param name="route">Route containing parameters.</param>
	public void Load(MainNavigationRoute route)
	{
		if (route.Type != MainNavigationRouteType.Feed)
			throw new Exception("Invalid route type.");

		if (_feed != null)
		{
			_feed.Loader.ItemsUpdated -= HandleItemsUpdated;
		}

		_feed = route.SelectedFeed!;
		_syncToken = _updateItemsToken = null;
		IsSyncInProgress = false;

		UpdateItems(reload: true);
		_feed.Loader.ItemsUpdated += HandleItemsUpdated;

		if (_feed.Loader.LastSynchronized == null)
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
	public ReadOnlyObservableCollection<IItemView> Items { get; }

	/// <summary>
	/// List of currently selected items.
	/// </summary>
	public ImmutableArray<IItemView> SelectedItems
	{
		get => _selectedItems;
		set
		{
			var oldItems = _selectedItems;
			if (SetProperty(ref _selectedItems, value) && !oldItems.SequenceEqual(value))
			{
				if (oldItems.Length == 1)
				{
					oldItems[0].PropertyChanged -= HandleItemPropertyChanged;
				}

				_loadItemContentToken = null;
				_openBrowserCommand.NotifyCanExecuteChanged();
				IsReloadContentAvailable = false;
				IsLoadContentInProgress = false;
				if (_selectedItems.Length == 1)
				{
					// Single selection automatically marks the item as "read"
					_currentToggleReadAction = ToggleReadAction.MarkRead;
					HandleToggleReadCommand();
					LoadItemContent();
					_selectedItems[0].PropertyChanged += HandleItemPropertyChanged;
				}
				else
				{
					CurrentRoute = FeedNavigationRoute.Selection(_selectedItems.Length);
					if (_selectedItems.Length > 1)
					{
						CurrentToggleReadAction = _selectedItems.All(item => item.IsRead)
							? ToggleReadAction.MarkUnread
							: ToggleReadAction.MarkRead;
					}
				}

				IsItemSelected = SelectedItems.Length >= 1;
			}
		}
	}

	/// <summary>
	/// View model used to present display options as a toolbar menu.
	/// </summary>
	public DisplayOptionsViewModel DisplayOptions { get; }

	/// <summary>
	/// Mode used for sorting the items.
	/// </summary>
	public ItemSortMode SelectedSortMode
	{
		get => _selectedSortMode;
		set
		{
			if (SetProperty(ref _selectedSortMode, value) && _items.Count != 0)
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
	/// Indicates if the app is currently loading the content for the selected item.
	/// </summary>
	public bool IsLoadContentInProgress
	{
		get => _isLoadContentInProgress;
		private set => SetProperty(ref _isLoadContentInProgress, value);
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
	
	/// <summary>
	/// The currently active navigation route.
	/// </summary>
	public FeedNavigationRoute CurrentRoute
	{
		get => _currentRoute;
		private set => SetProperty(ref _currentRoute, value);
	}

	private ToggleReadAction CurrentToggleReadAction
	{
		get => _currentToggleReadAction;
		set
		{
			if (_currentToggleReadAction != value)
			{
				_currentToggleReadAction = value;
				ToggleReadSymbol =
					value switch
					{
						ToggleReadAction.MarkRead => Symbol.MailRead,
						ToggleReadAction.MarkUnread => Symbol.MailUnread,
						_ => throw new IndexOutOfRangeException()
					};
			}
		}
	}

	private static IEnumerable<IItemView> SortItems(IEnumerable<IItemView> items, ItemSortMode sortMode)
	{
		return
			sortMode switch
			{
				ItemSortMode.Newest => items.OrderByDescending(item => item.PublishedTimestamp),
				ItemSortMode.Oldest => items.OrderBy(item => item.PublishedTimestamp),
				_ => items
			};
	}

	private static bool ShouldInsertSortedItem(IItemView existing, IItemView added, ItemSortMode sortMode)
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
	/// Load the content for the currently selected item.
	/// </summary>
	private async void LoadItemContent(bool reload = false)
	{
		if (SelectedItems.IsEmpty)
			return;

		var item = SelectedItems[0];
		var token = new object();
		_loadItemContentToken = token;

		var contentTask = item.LoadContentAsync(reload);
		if (!contentTask.IsCompletedSuccessfully)
		{
			IsLoadContentInProgress = true;
			IsReloadContentAvailable = false;
		}

		ItemContent content;
		try
		{
			content = await contentTask;
		}
		catch (Exception)
		{
			_modalService.Show(new ErrorViewModel(
				"Unable to load content", "An error occurred while trying to load the selected item's content."));
			IsLoadContentInProgress = false;
			return;
		}

		if (_loadItemContentToken == token)
		{
			CurrentRoute =
				content.Type switch
				{
					ItemContentType.Article => FeedNavigationRoute.Article(item, (ArticleItemContent)content),
					_ => throw new IndexOutOfRangeException()
				};
			IsLoadContentInProgress = false;
			IsReloadContentAvailable = content.IsReloadable;
		}
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
		var newItems = _feed.Loader.Items;
		var sortMode = SelectedSortMode;

		if (reload || _items.Count == 0)
		{
			_items.Clear();
			_itemSet = ImmutableHashSet<IItemView>.Empty;
			SelectedItems = ImmutableArray<IItemView>.Empty;

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
				ItemsUpdated?.Invoke(this, System.EventArgs.Empty);
			}
		}
		else
		{
			// Calculate the differences and sort new items on a background thread (see remarks above).
			var added = new List<IItemView>();
			var removed = ImmutableHashSet<IItemView>.Empty;
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
							_items.Insert(i, added[addedIndex++]);
							if (addedIndex >= added.Count)
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
				SelectedItems = SelectedItems.Where(i => !removed.Contains(i)).ToImmutableArray();
				ItemsUpdated?.Invoke(this, System.EventArgs.Empty);
			}
		}
	}

	private void HandleItemsUpdated(object? sender, FeedItemsUpdatedEventArgs e)
	{
		UpdateItems();
	}

	private void HandleItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		switch (e.PropertyName)
		{
			case nameof(IItemView.ContentLoader):
				LoadItemContent();
				break;
		}
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
				await _feed.Loader.SynchronizeAsync();
			}
			catch (Exception)
			{
				_modalService.Show(
					new ErrorViewModel(
						"Synchronization failed",
						"An error occurred while trying to synchronize your feeds. Please try again later."));
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

	private async void HandleToggleReadCommand()
	{
		var isRead = CurrentToggleReadAction == ToggleReadAction.MarkRead;
		CurrentToggleReadAction = isRead ? ToggleReadAction.MarkUnread : ToggleReadAction.MarkRead;
		foreach (var item in SelectedItems.Where(item => item.IsRead != isRead))
		{
			await item.Storage.SetItemReadAsync(item.Identifier, isRead);
		}
	}

	private async void HandleDeleteCommand()
	{
		// Execute deletes for each storage.
		var itemsPerStorage = new Dictionary<IItemStorage, List<Guid>>();
		foreach (var item in SelectedItems)
		{
			if (itemsPerStorage.TryGetValue(item.Storage, out var list))
			{
				list.Add(item.Identifier);
			}
			else
			{
				itemsPerStorage.Add(item.Storage, new List<Guid> { item.Identifier });
			}
		}

		foreach (var (storage, items) in itemsPerStorage)
		{
			await storage.DeleteItemsAsync(items);
		}
	}

	private void HandleReloadContentCommand()
	{
		LoadItemContent(reload: true);
	}

	private void HandleOpenBrowserCommand()
	{
		if (SelectedItems.IsEmpty)
			return;
		
		var url = SelectedItems[0].ContentUrl ?? SelectedItems[0].Url;
		if (url != null)
		{
			Process.Start(new ProcessStartInfo { UseShellExecute = true, FileName = url.ToString() });
		}
	}

	private readonly IModalService _modalService;
	private readonly IFeedService _feedService;
	private readonly RelayCommand _syncCommand;
	private readonly RelayCommand _toggleReadCommand;
	private readonly RelayCommand _deleteCommand;
	private readonly RelayCommand _reloadContentCommand;
	private readonly RelayCommand _openBrowserCommand;
	private readonly ObservableCollection<IItemView> _items = new();
	private ImmutableHashSet<IItemView> _itemSet = ImmutableHashSet<IItemView>.Empty;
	private object? _syncToken;
	private object? _updateItemsToken;
	private object? _loadItemContentToken;
	private IFeedView? _feed;
	private ImmutableArray<IItemView> _selectedItems = ImmutableArray<IItemView>.Empty;
	private ItemSortMode _selectedSortMode = ItemSortMode.Newest;
	private Symbol _toggleReadSymbol = Symbol.MailUnread;
	private bool _isSyncInProgress;
	private bool _isLoadContentInProgress;
	private bool _isItemSelected;
	private bool _isReloadContentAvailable;
	private FeedNavigationRoute _currentRoute = FeedNavigationRoute.Selection(0);
	private ToggleReadAction _currentToggleReadAction = ToggleReadAction.MarkUnread;
}
