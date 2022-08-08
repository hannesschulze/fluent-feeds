using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using FluentFeeds.Shared.Models;
using FluentFeeds.Shared.Models.Feeds;
using FluentFeeds.Shared.Models.Nodes;
using FluentFeeds.Shared.Services;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;

namespace FluentFeeds.Shared.ViewModels;

/// <summary>
/// View model for the main pages managing navigation between feeds and other pages.
/// </summary>
public class MainViewModel : ObservableObject
{
	/// <summary>
	/// Child pages of the main page.
	/// </summary>
	public enum Page
	{
		/// <summary>
		/// App settings
		/// </summary>
		Settings,
		/// <summary>
		/// Feed viewer
		/// </summary>
		Feed
	}

	public class SettingsItemViewModel : NavigationItemViewModel
	{
		public SettingsItemViewModel()
			: base("Settings", Symbol.Settings, isExpandable: false, NavigationRoute.Settings)
		{
		}
	}

	public class FeedItemViewModel : NavigationItemViewModel
	{
		public FeedItemViewModel(FeedItem item)
			: base(item.Title, item.Symbol, isExpandable: false, NavigationRoute.Feed(item))
		{
			Item = item;
		}

		public FeedItem Item { get; }
	}

	public MainViewModel(INavigationService navigationService)
	{
		_navigationService = navigationService;
		_navigationService.BackStackChanged += HandleBackStackChanged;

		_goBackCommand = new RelayCommand(() => _navigationService.GoBack(), () => _navigationService.CanGoBack);
		var overviewFeed = 
			_navigationService.BackStack[0].FeedItem ?? new FeedItem("Overview", Symbol.Home, new EmptyFeed());
		var unreadFeed = new FeedItem("Unread", Symbol.Sparkle, new EmptyFeed());
		_feedItems.Add(RegisterFeedItem(new FeedItemViewModel(overviewFeed)));
		_feedItems.Add(RegisterFeedItem(new FeedItemViewModel(unreadFeed)));
		_visiblePage = GetVisiblePage();
		_selectedItem = GetSelectedItem();

		FeedItems = new(_feedItems);
	}

	/// <summary>
	/// Go back to the previous page/feed/article.
	/// </summary>
	public ICommand GoBackCommand => _goBackCommand;

	/// <summary>
	/// The currently visible child page.
	/// </summary>
	public Page VisiblePage
	{
		get => _visiblePage;
		private set => SetProperty(ref _visiblePage, value);
	}

	/// <summary>
	/// View model of the navigation item currently selected.
	/// </summary>
	public NavigationItemViewModel? SelectedItem
	{
		get => _selectedItem;
		set
		{
			var destination = value?.Destination;
			if (!SetProperty(ref _selectedItem, value) || _isChangingSelection || destination == null)
				// Item is not actually selectable or the selection has already changed.
				return;

			_isChangingSelection = true;
			_navigationService.Navigate(destination.Value);
			_isChangingSelection = false;
		}
	}

	/// <summary>
	/// Navigation item for opening the app settings.
	/// </summary>
	public NavigationItemViewModel SettingsItem => _settingsItem;

	/// <summary>
	/// Observable collection containing items for all available feeds.
	/// </summary>
	public ReadOnlyObservableCollection<NavigationItemViewModel> FeedItems { get; }

	private void HandleBackStackChanged(object? sender, EventArgs e)
	{
		_goBackCommand.NotifyCanExecuteChanged();
		if (!_isChangingSelection)
		{
			_isChangingSelection = true;
			SelectedItem = GetSelectedItem();
			_isChangingSelection = false;
		}
		VisiblePage = GetVisiblePage();
	}

	private Page GetVisiblePage() =>
		_navigationService.CurrentRoute.Type switch
		{
			NavigationRouteType.Settings => Page.Settings,
			NavigationRouteType.Feed => Page.Feed,
			_ => throw new IndexOutOfRangeException()
		};

	private NavigationItemViewModel? GetSelectedItem() =>
		_navigationService.CurrentRoute.Type switch
		{
			NavigationRouteType.Settings => SettingsItem,
			NavigationRouteType.Feed =>
				_feedItemRegistry.GetValueOrDefault(_navigationService.CurrentRoute.FeedItem!),
			_ => null
		};

	private FeedItemViewModel RegisterFeedItem(FeedItemViewModel item)
	{
		_feedItemRegistry.Add(item.Item, item);
		if (item.Item == _navigationService.CurrentRoute.FeedItem)
		{
			_isChangingSelection = true;
			SelectedItem = item;
			_isChangingSelection = false;
		}
		return item;
	}

	private readonly INavigationService _navigationService;
	private readonly RelayCommand _goBackCommand;
	private readonly ObservableCollection<NavigationItemViewModel> _feedItems = new();
	private readonly SettingsItemViewModel _settingsItem = new();
	private readonly Dictionary<FeedItem, FeedItemViewModel> _feedItemRegistry = new();
	private Page _visiblePage;
	private NavigationItemViewModel? _selectedItem;
	private bool _isChangingSelection;
}
