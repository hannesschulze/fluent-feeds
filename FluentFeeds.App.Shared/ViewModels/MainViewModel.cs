using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using FluentFeeds.App.Shared.Helpers;
using FluentFeeds.App.Shared.Models;
using FluentFeeds.App.Shared.Services;
using FluentFeeds.App.Shared.ViewModels.Main;
using FluentFeeds.Common;
using FluentFeeds.Feeds.Base;
using FluentFeeds.Feeds.Base.Nodes;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;

namespace FluentFeeds.App.Shared.ViewModels;

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

	public MainViewModel(IFeedService feedService, INavigationService navigationService)
	{
		_feedService = feedService;
		_navigationService = navigationService;
		_navigationService.BackStackChanged += HandleBackStackChanged;
		
		SettingsItem =
			new MainItemViewModel(NavigationRoute.Settings, isExpandable: false, "Settings", Symbol.Settings);
		FeedItems = new ReadOnlyObservableCollection<MainItemViewModel>(_feedItems);

		_goBackCommand = new RelayCommand(() => _navigationService.GoBack(), () => _navigationService.CanGoBack);
		var overviewFeedNode = feedService.OverviewFeed;
		var overviewItem = new MainFeedItemViewModel(overviewFeedNode, null, _feedItemRegistry);
		var unreadFeedNode = FeedNode.Custom(new EmptyFeed(), "Unread", Symbol.Sparkle, false);
		var unreadItem = new MainFeedItemViewModel(unreadFeedNode, null, _feedItemRegistry);
		_feedItemRegistry.Add(overviewFeedNode, overviewItem);
		_feedItemRegistry.Add(unreadFeedNode, unreadItem);
		_feedItems.Add(overviewItem);
		_feedItems.Add(unreadItem);
		_feedItemTransformer = ObservableCollectionTransformer.CreateCached(
			feedService.FeedProviders, _feedItems, 
			provider => new MainFeedItemViewModel(provider.RootNode, provider, _feedItemRegistry), 
			provider => provider.RootNode, _feedItemRegistry);
		_visiblePage = GetVisiblePage();
		_selectedItem = GetSelectedItem();
		
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
	public MainItemViewModel? SelectedItem
	{
		get => _selectedItem;
		set
		{
			var destination = (value as MainItemViewModel)?.Destination;
			if (!SetProperty(ref _selectedItem, value) || _isChangingSelection || destination == null)
				// Item is not actually selectable or the selection has already changed.
				return;

			_isChangingSelection = true;
			_navigationService.Navigate(destination.Value);
			_isChangingSelection = false;
		}
	}

	/// <summary>
	/// List item for opening the app settings.
	/// </summary>
	public MainItemViewModel SettingsItem { get; }

	/// <summary>
	/// Observable collection containing items for all available feeds.
	/// </summary>
	public ReadOnlyObservableCollection<MainItemViewModel> FeedItems { get; }

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

	private MainItemViewModel? GetSelectedItem() =>
		_navigationService.CurrentRoute.Type switch
		{
			NavigationRouteType.Settings => SettingsItem,
			NavigationRouteType.Feed =>
				_feedItemRegistry.GetValueOrDefault(_navigationService.CurrentRoute.FeedNode!),
			_ => null
		};

	private readonly IFeedService _feedService;
	private readonly INavigationService _navigationService;
	private readonly RelayCommand _goBackCommand;
	private readonly ObservableCollection<MainItemViewModel> _feedItems = new();
	private readonly ObservableCollectionTransformer<LoadedFeedProvider, MainItemViewModel> _feedItemTransformer;
	private readonly Dictionary<IReadOnlyFeedNode, MainItemViewModel> _feedItemRegistry = new();
	private Page _visiblePage;
	private MainItemViewModel? _selectedItem;
	private bool _isChangingSelection;
}
