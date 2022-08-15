using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using FluentFeeds.App.Shared.Helpers;
using FluentFeeds.App.Shared.Models;
using FluentFeeds.App.Shared.Services;
using FluentFeeds.Common;
using FluentFeeds.Feeds.Base;
using FluentFeeds.Feeds.Base.Nodes;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;

namespace FluentFeeds.App.Shared.ViewModels;

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
		public FeedItemViewModel(
			IReadOnlyFeedNode feedNode, LoadedFeedProvider? feedProvider,
			Dictionary<IReadOnlyFeedNode, NavigationItemViewModel> feedItemRegistry) : base(
			feedNode.ActualTitle ?? "Unnamed", feedNode.ActualSymbol ?? Symbol.Feed,
			isExpandable: feedNode.Children != null, NavigationRoute.Feed(feedNode))
		{
			_feedNode = feedNode;
			
			feedNode.PropertyChanged += HandlePropertyChanged;
			if (feedNode.Children != null)
			{
				ObservableCollectionTransformer.CreateCached(
					feedNode.Children, MutableChildren, 
					node => new FeedItemViewModel(node, feedProvider, feedItemRegistry), feedItemRegistry);
			}
		}

		private void HandlePropertyChanged(object? sender, PropertyChangedEventArgs e)
		{
			switch (e.PropertyName)
			{
				case nameof(IReadOnlyFeedNode.ActualTitle):
					Title = _feedNode.ActualTitle ?? "Unnamed";
					break;
				case nameof(IReadOnlyFeedNode.ActualSymbol):
					Symbol = _feedNode.ActualSymbol ?? Symbol.Feed;
					break;
			}
		}

		private readonly IReadOnlyFeedNode _feedNode;
	}

	public MainViewModel(IFeedService feedService, INavigationService navigationService)
	{
		_navigationService = navigationService;
		_navigationService.BackStackChanged += HandleBackStackChanged;

		_goBackCommand = new RelayCommand(() => _navigationService.GoBack(), () => _navigationService.CanGoBack);
		var overviewFeedNode = feedService.OverviewFeed;
		var overviewItem = new FeedItemViewModel(overviewFeedNode, null, _feedItemRegistry);
		var unreadFeedNode = FeedNode.Custom(new EmptyFeed(), "Unread", Symbol.Sparkle, false);
		var unreadItem = new FeedItemViewModel(unreadFeedNode, null, _feedItemRegistry);
		_feedItemRegistry.Add(overviewFeedNode, overviewItem);
		_feedItemRegistry.Add(unreadFeedNode, unreadItem);
		_feedItems.Add(overviewItem);
		_feedItems.Add(unreadItem);
		_feedItemTransformer = ObservableCollectionTransformer.CreateCached(
			feedService.FeedProviders, _feedItems, 
			provider => new FeedItemViewModel(provider.RootNode, provider, _feedItemRegistry), 
			provider => provider.RootNode, _feedItemRegistry);
		_visiblePage = GetVisiblePage();
		_selectedItem = GetSelectedItem();

		FeedItems = new ReadOnlyObservableCollection<NavigationItemViewModel>(_feedItems);

		LoadFeeds(feedService);
	}

	private async void LoadFeeds(IFeedService feedService)
	{
		await feedService.InitializeAsync();
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
				_feedItemRegistry.GetValueOrDefault(_navigationService.CurrentRoute.FeedNode!),
			_ => null
		};

	private readonly INavigationService _navigationService;
	private readonly RelayCommand _goBackCommand;
	private readonly ObservableCollection<NavigationItemViewModel> _feedItems = new();
	private readonly ObservableCollectionTransformer<LoadedFeedProvider, NavigationItemViewModel> _feedItemTransformer;
	private readonly SettingsItemViewModel _settingsItem = new();
	private readonly Dictionary<IReadOnlyFeedNode, NavigationItemViewModel> _feedItemRegistry = new();
	private Page _visiblePage;
	private NavigationItemViewModel? _selectedItem;
	private bool _isChangingSelection;
}
