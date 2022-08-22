using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using FluentFeeds.App.Shared.Helpers;
using FluentFeeds.App.Shared.Models;
using FluentFeeds.App.Shared.Services;
using FluentFeeds.App.Shared.ViewModels.Items.Navigation;
using FluentFeeds.App.Shared.ViewModels.Modals;
using FluentFeeds.Common;
using FluentFeeds.Feeds.Base;
using FluentFeeds.Feeds.Base.Nodes;
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
		_settingsItem =
			new NavigationItemViewModel(NavigationRoute.Settings, isExpandable: false, "Settings", Symbol.Settings);
		_footerItems.Add(_settingsItem);
		var overviewFeedNode = feedService.OverviewNode;
		var overviewItem = new FeedNavigationItemViewModel(modalService, overviewFeedNode, null, _feedItemRegistry);
		var unreadFeedNode = FeedNode.Custom(new EmptyFeed(), "Unread", Symbol.Sparkle, false);
		var unreadItem = new FeedNavigationItemViewModel(modalService, unreadFeedNode, null, _feedItemRegistry);
		_feedItemRegistry.Add(overviewFeedNode, overviewItem);
		_feedItemRegistry.Add(unreadFeedNode, unreadItem);
		_feedItems.Add(overviewItem);
		_feedItems.Add(unreadItem);
		_feedItemTransformer = ObservableCollectionTransformer.CreateCached(
			feedService.ProviderNodes, _feedItems,
			rootNode => new FeedNavigationItemViewModel(modalService, rootNode, rootNode, _feedItemRegistry),
			rootNode => rootNode, _feedItemRegistry);
		_currentRoute = NavigationRoute.Feed(overviewFeedNode);
		_backStack.Add(_currentRoute);
		_selectedItem = overviewItem;

		FooterItems = new ReadOnlyObservableCollection<NavigationItemViewModel>(_footerItems);
		FeedItems = new ReadOnlyObservableCollection<NavigationItemViewModel>(_feedItems);

		InitializeFeeds();
	}

	private async void InitializeFeeds()
	{
		try
		{
			await _feedService.InitializeAsync();
		}
		catch (Exception)
		{
			_modalService.Show(
				new ErrorViewModel("A database error occurred", "FluentFeeds was unable to initialize its database."));
		}
	}

	/// <summary>
	/// Go back to the previous page/feed.
	/// </summary>
	public ICommand GoBackCommand => _goBackCommand;

	/// <summary>
	/// The currently active navigation route.
	/// </summary>
	public NavigationRoute CurrentRoute
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
		var newRoute = _backStack[^1];
		_goBackCommand.NotifyCanExecuteChanged();
		_isChangingSelection = true;
		SelectedItem =
			newRoute.Type switch
			{
				NavigationRouteType.Settings => _settingsItem,
				NavigationRouteType.Feed => _feedItemRegistry.GetValueOrDefault(newRoute.FeedNode!),
				_ => null
			};
		_isChangingSelection = false;
		CurrentRoute = newRoute;
	}

	private readonly IFeedService _feedService;
	private readonly IModalService _modalService;
	private readonly List<NavigationRoute> _backStack = new();
	private readonly RelayCommand _goBackCommand;
	private readonly NavigationItemViewModel _settingsItem;
	private readonly ObservableCollection<NavigationItemViewModel> _feedItems = new();
	private readonly ObservableCollection<NavigationItemViewModel> _footerItems = new();
	private readonly ObservableCollectionTransformer<IReadOnlyStoredFeedNode, NavigationItemViewModel> _feedItemTransformer;
	private readonly Dictionary<IReadOnlyFeedNode, NavigationItemViewModel> _feedItemRegistry = new();
	private NavigationRoute _currentRoute;
	private NavigationItemViewModel? _selectedItem;
	private bool _isChangingSelection;
}
