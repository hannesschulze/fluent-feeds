using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using FluentFeeds.App.Shared.Helpers;
using FluentFeeds.App.Shared.Models;
using FluentFeeds.App.Shared.Services;
using FluentFeeds.App.Shared.ViewModels.Items.Navigation;
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

	public MainViewModel(IFeedService feedService, INavigationService navigationService, IModalService modalService)
	{
		_feedService = feedService;
		_navigationService = navigationService;
		_navigationService.BackStackChanged += HandleBackStackChanged;
		
		_goBackCommand = new RelayCommand(() => _navigationService.GoBack(), () => _navigationService.CanGoBack);
		_settingsItem =
			new NavigationItemViewModel(NavigationRoute.Settings, isExpandable: false, "Settings", Symbol.Settings);
		_footerItems.Add(_settingsItem);
		var overviewFeedNode = feedService.OverviewFeed;
		var overviewItem = new FeedNavigationItemViewModel(
			feedService, modalService, overviewFeedNode, null, _feedItemRegistry);
		var unreadFeedNode = FeedNode.Custom(new EmptyFeed(), "Unread", Symbol.Sparkle, false);
		var unreadItem = new FeedNavigationItemViewModel(
			feedService, modalService, unreadFeedNode, null, _feedItemRegistry);
		_feedItemRegistry.Add(overviewFeedNode, overviewItem);
		_feedItemRegistry.Add(unreadFeedNode, unreadItem);
		_feedItems.Add(overviewItem);
		_feedItems.Add(unreadItem);
		_feedItemTransformer = ObservableCollectionTransformer.CreateCached(
			feedService.FeedProviders, _feedItems, 
			provider => new FeedNavigationItemViewModel(
				feedService, modalService, provider.RootNode, provider, _feedItemRegistry),
			provider => provider.RootNode, _feedItemRegistry);
		_visiblePage = GetVisiblePage();
		_selectedItem = GetSelectedItem();

		FooterItems = new ReadOnlyObservableCollection<NavigationItemViewModel>(_footerItems);
		FeedItems = new ReadOnlyObservableCollection<NavigationItemViewModel>(_feedItems);

		InitializeFeeds();
	}

	private async void InitializeFeeds()
	{
		await _feedService.InitializeAsync();
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
			if (!SetProperty(ref _selectedItem, value) || _isChangingSelection || value == null)
				// Item is not actually selectable or the selection has already changed.
				return;

			_isChangingSelection = true;
			_navigationService.Navigate(value.Destination);
			_isChangingSelection = false;
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
			NavigationRouteType.Settings => _settingsItem,
			NavigationRouteType.Feed =>
				_feedItemRegistry.GetValueOrDefault(_navigationService.CurrentRoute.FeedNode!),
			_ => null
		};

	private readonly IFeedService _feedService;
	private readonly INavigationService _navigationService;
	private readonly RelayCommand _goBackCommand;
	private readonly NavigationItemViewModel _settingsItem;
	private readonly ObservableCollection<NavigationItemViewModel> _feedItems = new();
	private readonly ObservableCollection<NavigationItemViewModel> _footerItems = new();
	private readonly ObservableCollectionTransformer<LoadedFeedProvider, NavigationItemViewModel> _feedItemTransformer;
	private readonly Dictionary<IReadOnlyFeedNode, NavigationItemViewModel> _feedItemRegistry = new();
	private Page _visiblePage;
	private NavigationItemViewModel? _selectedItem;
	private bool _isChangingSelection;
}
