using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using FluentFeeds.App.Shared.EventArgs;
using FluentFeeds.App.Shared.Helpers;
using FluentFeeds.App.Shared.Models.Feeds;
using FluentFeeds.App.Shared.Models.Feeds.Loaders;
using FluentFeeds.App.Shared.Models.Navigation;
using FluentFeeds.App.Shared.Resources;
using FluentFeeds.App.Shared.Services;
using FluentFeeds.App.Shared.ViewModels.ListItems.Navigation;
using FluentFeeds.App.Shared.ViewModels.Modals;
using FluentFeeds.Common;
using FluentFeeds.Feeds.Base.Feeds.Content;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;

namespace FluentFeeds.App.Shared.ViewModels.Pages;

/// <summary>
/// View model for the main page managing navigation between feeds and other pages.
/// </summary>
public sealed class MainViewModel : ObservableObject
{
	public MainViewModel(IFeedService feedService, IModalService modalService)
	{
		_feedService = feedService;
		_modalService = modalService;
		
		_goBackCommand = new RelayCommand(HandleGoBackCommand, () => _backStack.Count > 1);
		_searchCommand = new RelayCommand(HandleSearchCommand);
		_settingsItem = new NavigationItemViewModel(
			MainNavigationRoute.Settings, isExpandable: false, LocalizedStrings.SettingsTitle, Symbol.Settings);
		_footerItems.Add(_settingsItem);
		var overviewItem = new FeedNavigationItemViewModel(
			modalService, feedService.OverviewFeed, null, _feedItemRegistry);
		var searchFeed = new Feed(
			identifier: Guid.Parse("604d90a3-3aed-43e4-8c4f-a371511cc374"),
			storage: null,
			loaderFactory: CreateSearchFeedLoader,
			hasChildren: false,
			parent: null,
			name: LocalizedStrings.BuiltInFeedSearchName,
			symbol: Symbol.Search,
			metadata: new FeedMetadata(),
			isUserCustomizable: false,
			isExcludedFromGroup: false);
		_searchItem = new FeedNavigationItemViewModel(modalService, searchFeed, null, _feedItemRegistry);
		_feedItemRegistry.Add(feedService.OverviewFeed, overviewItem);
		_feedItemRegistry.Add(searchFeed, _searchItem);
		_feedItems.Add(overviewItem);
		_feedItemTransformer = ObservableCollectionTransformer.CreateCached(
			feedService.ProviderFeeds, _feedItems, CreateRootFeedViewModel, _feedItemRegistry);
		_currentRoute = MainNavigationRoute.Feed(feedService.OverviewFeed);
		_backStack.Add(_currentRoute);
		_selectedItem = overviewItem;

		FooterItems = new ReadOnlyObservableCollection<NavigationItemViewModel>(_footerItems);
		FeedItems = new ReadOnlyObservableCollection<NavigationItemViewModel>(_feedItems);

		InitializeFeeds();
	}

	public bool HasBuggySelection { get; set; } = true;

	private async void InitializeFeeds()
	{
		try
		{
			await _feedService.InitializeAsync();
		}
		catch (Exception e)
		{
			Console.Error.WriteLine($"Unable to initialize feeds: {e}");
			_modalService.Show(
				new ErrorViewModel(
					LocalizedStrings.InitializeDatabaseErrorTitle,
					String.Format(LocalizedStrings.InitializeDatabaseErrorMessage, Constants.AppName)));
		}
	}

	private NavigationItemViewModel CreateRootFeedViewModel(IFeedView rootFeed)
	{
		if (rootFeed.Storage != null)
		{
			rootFeed.Storage.FeedsDeleted += HandleFeedsDeleted;
		}
		return new FeedNavigationItemViewModel(_modalService, rootFeed, rootFeed, _feedItemRegistry);
	}

	/// <summary>
	/// Go back to the previous page/feed.
	/// </summary>
	public ICommand GoBackCommand => _goBackCommand;

	/// <summary>
	/// Start a new search.
	/// </summary>
	public ICommand SearchCommand => _searchCommand;

	/// <summary>
	/// The window title.
	/// </summary>
	public string Title => Constants.AppName;

	/// <summary>
	/// Placeholder text shown in the search field if it is empty.
	/// </summary>
	public string SearchPlaceholder => LocalizedStrings.SearchPlaceholder;

	/// <summary>
	/// Current text value of the search field.
	/// </summary>
	public string SearchText
	{
		get => _searchText;
		set
		{
			if (SetProperty(ref _searchText, value) && String.IsNullOrEmpty(value))
			{
				HandleSearchCommand();
			}
		}
	}

	/// <summary>
	/// The currently active navigation route.
	/// </summary>
	public MainNavigationRoute CurrentRoute
	{
		get => _currentRoute;
		private set => SetProperty(ref _currentRoute, value);
	}

	/// <summary>
	/// View model of the navigation item currently selected.
	/// </summary>
	public NavigationItemViewModel? SelectedItem
	{
		get => _selectedItem;
		set
		{
			if (!SetProperty(ref _selectedItem, value) || _isChangingSelection || value == null)
				// Item is not actually selectable or the selection has already changed.
				return;

			var newRoute = value.Destination;
			_backStack.Add(newRoute);
			_goBackCommand.NotifyCanExecuteChanged();
			CurrentRoute = newRoute;
		}
	}

	/// <summary>
	/// Observable collection containing items for all available feeds.
	/// </summary>
	public ReadOnlyObservableCollection<NavigationItemViewModel> FeedItems { get; }

	/// <summary>
	/// Observable collection containing items displayed at the bottom of the navigation menu.
	/// </summary>
	public ReadOnlyObservableCollection<NavigationItemViewModel> FooterItems { get; }

	private void HandleGoBackCommand()
	{
		_backStack.RemoveAt(_backStack.Count - 1);
		HandleBackStackChanged();
	}

	private void HandleBackStackChanged()
	{
		var newRoute = _backStack[^1];
		_goBackCommand.NotifyCanExecuteChanged();
		_isChangingSelection = true;
		SelectedItem =
			newRoute.Type switch
			{
				MainNavigationRouteType.Settings => _settingsItem,
				MainNavigationRouteType.Feed => _feedItemRegistry.GetValueOrDefault(newRoute.SelectedFeed!),
				_ => null
			};
		_isChangingSelection = false;
		CurrentRoute = newRoute;
	}

	private void RemoveFeedsFromBackStack(ImmutableHashSet<IFeedView> feeds)
	{
		bool changed = false;
		MainNavigationRoute? previousRoute = null;
		for (var i = _backStack.Count - 1; i >= 0; --i)
		{
			var route = _backStack[i];
			if (route == previousRoute || (route.SelectedFeed != null && feeds.Contains(route.SelectedFeed)))
			{
				_backStack.RemoveAt(i);
				changed = true;
			}
			else
			{
				previousRoute = route;
			}
		}

		if (changed)
		{
			HandleBackStackChanged();
		}
	}
	
	private void HandleFeedsDeleted(object? sender, FeedsDeletedEventArgs e)
	{
		var feeds = e.Feeds.ToImmutableHashSet();

		// Clean up cache
		foreach (var feed in feeds)
		{
			_feedItemRegistry.Remove(feed);
		}

		// Remove feeds from the back stack
		RemoveFeedsFromBackStack(feeds);
	}

	private async void Search(ImmutableArray<string> searchTerms)
	{
		var wasEmpty = _searchTerms.IsEmpty;
		var isEmpty = searchTerms.IsEmpty;
		_searchTerms = searchTerms;
		if (_searchFeedLoader != null)
		{
			_searchFeedLoader.SearchTerms = searchTerms;
		}

		if (!isEmpty)
		{
			if (wasEmpty)
			{
				_feedItems.Insert(1, _searchItem);
				_feedItemTransformer.TargetOffset++;
				// HACK: This schedules the selection on the current synchronization context to work around the navigation
				// view not being ready to update the selection.
				if (HasBuggySelection)
				{
					await Task.Delay(50);
				}
			}
			SelectedItem = _searchItem;
		}
		else if (!wasEmpty && isEmpty)
		{
			RemoveFeedsFromBackStack(ImmutableHashSet.Create(_searchItem.Feed));
			_feedItems.RemoveAt(1);
			_feedItemTransformer.TargetOffset--;
		}
	}

	private void HandleSearchCommand()
	{
		var searchTerms = String.IsNullOrEmpty(SearchText) 
			? ImmutableArray<string>.Empty
			: SearchText
				.Split(' ')
				.Select(term => term.Trim())
				.Where(term => !String.IsNullOrEmpty(term))
				.ToImmutableArray();
		Search(searchTerms);
	}

	private FeedLoader CreateSearchFeedLoader(IFeedView feed)
	{
		_searchFeedLoader = new SearchFeedLoader(_feedService.OverviewFeed.Loader) { SearchTerms = _searchTerms };
		return _searchFeedLoader;
	}

	private readonly IFeedService _feedService;
	private readonly IModalService _modalService;
	private readonly List<MainNavigationRoute> _backStack = new();
	private readonly RelayCommand _goBackCommand;
	private readonly RelayCommand _searchCommand;
	private readonly NavigationItemViewModel _settingsItem;
	private readonly FeedNavigationItemViewModel _searchItem;
	private readonly ObservableCollection<NavigationItemViewModel> _feedItems = new();
	private readonly ObservableCollection<NavigationItemViewModel> _footerItems = new();
	private readonly ObservableCollectionTransformer<IFeedView, NavigationItemViewModel> _feedItemTransformer;
	private readonly Dictionary<IFeedView, NavigationItemViewModel> _feedItemRegistry = new();
	private SearchFeedLoader? _searchFeedLoader;
	private ImmutableArray<string> _searchTerms = ImmutableArray<string>.Empty;
	private string _searchText = String.Empty;
	private MainNavigationRoute _currentRoute;
	private NavigationItemViewModel? _selectedItem;
	private bool _isChangingSelection;
}
