using System;
using System.Collections.Generic;
using FluentFeeds.App.Shared.Models;
using FluentFeeds.App.Shared.Services.Default;
using FluentFeeds.App.Shared.Tests.Services.Mock;
using FluentFeeds.App.Shared.ViewModels.Main;
using FluentFeeds.Common;
using FluentFeeds.Feeds.Base;
using FluentFeeds.Feeds.Base.Nodes;
using Xunit;

namespace FluentFeeds.App.Shared.Tests.ViewModels;

public class MainViewModelTests
{
	public MainViewModelTests()
	{
		FeedService = new FeedServiceMock();
		NavigationService = new NavigationService(FeedService);
	}
	
	private FeedServiceMock FeedService { get; }
	private NavigationService NavigationService { get; }

	[Fact]
	public void ManagesNavigationItems()
	{
		var viewModel = new MainViewModel(FeedService, NavigationService);
		var feedStorage = new FeedStorageMock();
		var provider = new FeedProviderMock(Guid.Empty);
		var node = provider.CreateInitialTree(feedStorage);
		var loadedProvider = new LoadedFeedProvider(provider, provider.CreateInitialTree(feedStorage), feedStorage);
		FeedService.FeedProviders.Add(loadedProvider);

		Assert.Collection(
			viewModel.FeedItems,
			item =>
			{
				var feedItem = Assert.IsType<FeedNavigationItemViewModel>(item);
				Assert.Equal(FeedService.OverviewFeed, feedItem.FeedNode);
				Assert.Null(feedItem.FeedProvider);
			},
			item =>
			{
				var feedItem = Assert.IsType<FeedNavigationItemViewModel>(item);
				Assert.Equal("Unread", feedItem.Title);
				Assert.Null(feedItem.FeedProvider);
			},
			item =>
			{
				var feedItem = Assert.IsType<FeedNavigationItemViewModel>(item);
				Assert.Equal(node, feedItem.FeedNode);
				Assert.Equal(loadedProvider, feedItem.FeedProvider);
			});

		var settings = viewModel.SettingsItem;
		Assert.Equal("Settings", settings.Title);
		Assert.Equal(Symbol.Settings, settings.Symbol);
		Assert.Equal(NavigationRoute.Settings, settings.Destination);
		Assert.False(settings.IsExpandable);
		Assert.True(settings.IsSelectable);
		Assert.Empty(settings.Children);
	}
	
	[Fact]
	public void FeedNavigationItems_GroupNode()
	{
		var node = FeedNode.Group("Title", Symbol.Directory, isUserCustomizable: true);
		var item = new FeedNavigationItemViewModel(
			node, null, new Dictionary<IReadOnlyFeedNode, NavigationItemViewModel>());
		Assert.Equal("Title",item.Title);
		Assert.Equal(Symbol.Directory, item.Symbol);
		Assert.Equal(NavigationRoute.Feed(node), item.Destination);
		Assert.True(item.IsExpandable);
		Assert.True(item.IsSelectable);
		Assert.Empty(item.Children);

		var childNode = FeedNode.Group(null, null, false);
		node.Children!.Add(childNode);
		var childItem = Assert.IsType<FeedNavigationItemViewModel>(Assert.Single(item.Children));
		Assert.Equal(childNode, childItem.FeedNode);
	}
	
	[Fact]
	public void FeedNavigationItems_LeafNode()
	{
		var feed = new FeedMock(Guid.Empty);
		feed.UpdateMetadata(new FeedMetadata { Name = "Feed", Symbol = Symbol.Web });
		var node = FeedNode.Custom(feed, null, null, isUserCustomizable: true);
		var item = new FeedNavigationItemViewModel(
			node, null, new Dictionary<IReadOnlyFeedNode, NavigationItemViewModel>());
		Assert.Equal("Feed",item.Title);
		Assert.Equal(Symbol.Web, item.Symbol);
		Assert.Equal(NavigationRoute.Feed(node), item.Destination);
		Assert.False(item.IsExpandable);
		Assert.True(item.IsSelectable);
		Assert.Empty(item.Children);
	}

	[Fact]
	public void FeedNavigationItems_UpdateProperties()
	{
		var feed = new FeedMock(Guid.Empty);
		feed.UpdateMetadata(new FeedMetadata { Name = "Feed", Symbol = Symbol.Web });
		var node = FeedNode.Custom(feed, null, null, isUserCustomizable: true);
		var item = new FeedNavigationItemViewModel(
			node, null, new Dictionary<IReadOnlyFeedNode, NavigationItemViewModel>());
		feed.UpdateMetadata(new FeedMetadata { Name = "Updated", Symbol = Symbol.Feed });
		Assert.Equal("Updated",item.Title);
		Assert.Equal(Symbol.Feed, item.Symbol);
	}

	[Fact]
	public void UpdatesSelectionWhenPageChanges_ToOtherFeed()
	{
		var viewModel = new MainViewModel(FeedService, NavigationService);
		Assert.Equal(viewModel.FeedItems[0], viewModel.SelectedItem);
		var unread = viewModel.FeedItems[1];
		NavigationService.Navigate(unread.Destination!.Value);
		Assert.Equal(unread, viewModel.SelectedItem);
	}

	[Fact]
	public void UpdatesSelectionWhenPageChanges_ToSettings()
	{
		var viewModel = new MainViewModel(FeedService, NavigationService);
		Assert.Equal(viewModel.FeedItems[0], viewModel.SelectedItem);
		NavigationService.Navigate(NavigationRoute.Settings);
		Assert.Equal(viewModel.SettingsItem, viewModel.SelectedItem);
	}

	[Fact]
	public void UpdatesSelectionWhenPageChanges_ToUnknownRoute()
	{
		var viewModel = new MainViewModel(FeedService, NavigationService);
		Assert.Equal(viewModel.FeedItems[0], viewModel.SelectedItem);
		NavigationService.Navigate(NavigationRoute.Feed(
			FeedNode.Custom(new EmptyFeed(), "Foo", Symbol.Feed, false)));
		Assert.Null(viewModel.SelectedItem);
	}

	[Fact]
	public void UpdatesPageWhenSelectionChanges()
	{
		var viewModel = new MainViewModel(FeedService, NavigationService);
		viewModel.SelectedItem = viewModel.SettingsItem;
		Assert.Equal(viewModel.SettingsItem, viewModel.SelectedItem);
		Assert.Equal(NavigationRoute.Settings, NavigationService.CurrentRoute);
	}

	[Fact]
	public void UpdatesVisiblePage()
	{
		var viewModel = new MainViewModel(FeedService, NavigationService);
		Assert.Equal(MainViewModel.Page.Feed, viewModel.VisiblePage);
		NavigationService.Navigate(NavigationRoute.Settings);
		Assert.Equal(MainViewModel.Page.Settings, viewModel.VisiblePage);
	}

	[Fact]
	public void UpdatesVisiblePage_OnlyForDifferentRouteType()
	{
		var viewModel = new MainViewModel(FeedService, NavigationService);
		viewModel.PropertyChanged +=
			(s, e) => Assert.NotEqual(nameof(MainViewModel.VisiblePage), e.PropertyName);
		NavigationService.Navigate(viewModel.FeedItems[1].Destination!.Value);
		Assert.Equal(MainViewModel.Page.Feed, viewModel.VisiblePage);
	}

	[Fact]
	public void GoBack_UpdatesAvailability()
	{
		var viewModel = new MainViewModel(FeedService, NavigationService);
		Assert.False(viewModel.GoBackCommand.CanExecute(null));
		var changed = false;
		viewModel.GoBackCommand.CanExecuteChanged += (s, e) => changed = true;
		NavigationService.Navigate(NavigationRoute.Settings);
		Assert.True(changed);
		Assert.True(viewModel.GoBackCommand.CanExecute(null));
	}

	[Fact]
	public void GoBack_ExecuteCommand()
	{
		var viewModel = new MainViewModel(FeedService, NavigationService);
		NavigationService.Navigate(NavigationRoute.Settings);
		viewModel.GoBackCommand.Execute(null);
		Assert.Equal(viewModel.FeedItems[0].Destination, NavigationService.CurrentRoute);
		Assert.False(viewModel.GoBackCommand.CanExecute(null));
	}
}
