using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Windows.Input;
using FluentFeeds.App.Shared.Models;
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
	public FeedViewModel()
	{
		_syncCommand = new RelayCommand(() => { });
		_markReadCommand = new RelayCommand(() => { });
		_deleteCommand = new RelayCommand(() => { });
		_openDisplayOptionsCommand = new RelayCommand(() => { });
		_reloadContentCommand = new RelayCommand(() => { });
		_openBrowserCommand = new RelayCommand(() => { });
		
		Items = new ReadOnlyObservableCollection<IReadOnlyItem>(_items);
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
	/// Mark the currently selected item(s) as "read".
	/// </summary>
	public ICommand MarkReadCommand => _markReadCommand;

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
	public ReadOnlyObservableCollection<IReadOnlyItem> Items { get; }

	/// <summary>
	/// List of currently selected items.
	/// </summary>
	public ImmutableArray<IReadOnlyItem> SelectedItems
	{
		get => _selectedItems;
		set => SetProperty(ref _selectedItems, value);
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
	/// Flag indicating if the option to execute <see cref="MarkReadCommand"/> should be shown.
	/// </summary>
	/// <remarks>
	/// The option is hidden if a single item or no items are selected because those items are marked as "read"
	/// automatically.
	/// </remarks>
	public bool IsMarkReadAvailable
	{
		get => _isMarkReadAvailable;
		private set => SetProperty(ref _isMarkReadAvailable, value);
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
		private set => SetProperty(ref _isReloadContentAvailable, value);
	}

	private readonly RelayCommand _syncCommand;
	private readonly RelayCommand _markReadCommand;
	private readonly RelayCommand _deleteCommand;
	private readonly RelayCommand _openDisplayOptionsCommand;
	private readonly RelayCommand _reloadContentCommand;
	private readonly RelayCommand _openBrowserCommand;
	private readonly ObservableCollection<IReadOnlyItem> _items = new();
	private ImmutableArray<IReadOnlyItem> _selectedItems = ImmutableArray<IReadOnlyItem>.Empty;
	private ItemSortMode _selectedSortMode = ItemSortMode.Newest;
	private bool _isMarkReadAvailable;
	private bool _isReloadContentAvailable;
}
