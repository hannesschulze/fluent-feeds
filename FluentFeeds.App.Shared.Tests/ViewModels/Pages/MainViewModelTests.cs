using System;
using FluentFeeds.App.Shared.Models;
using FluentFeeds.App.Shared.Tests.Mock;
using FluentFeeds.App.Shared.ViewModels.Items.Navigation;
using FluentFeeds.App.Shared.ViewModels.Pages;
using FluentFeeds.Common;
using Xunit;

namespace FluentFeeds.App.Shared.Tests.ViewModels.Pages;

public class MainViewModelTests
{
	public MainViewModelTests()
	{
		ModalService = new ModalServiceMock();
		FeedService = new FeedServiceMock();
	}
	
	private ModalServiceMock ModalService { get; }
	private FeedServiceMock FeedService { get; }

	[Fact]
	public void ManagesNavigationItems()
	{
		var viewModel = new MainViewModel(FeedService, ModalService);
		var provider = new FeedProviderMock(Guid.Empty);
		var feedStorage = new FeedStorageMock(provider);
		var node = feedStorage.AddRootNode(provider.CreateInitialTree(feedStorage));
		FeedService.ProviderNodes.Add(node);
		FeedService.CompleteInitialization();

		Assert.Collection(
			viewModel.FeedItems,
			item =>
			{
				var feedItem = Assert.IsType<FeedNavigationItemViewModel>(item);
				Assert.Equal(FeedService.OverviewNode, feedItem.FeedNode);
				Assert.Null(feedItem.RootNode);
			},
			item =>
			{
				var feedItem = Assert.IsType<FeedNavigationItemViewModel>(item);
				Assert.Equal("Unread", feedItem.Title);
				Assert.Null(feedItem.RootNode);
			},
			item =>
			{
				var feedItem = Assert.IsType<FeedNavigationItemViewModel>(item);
				Assert.Equal(node, feedItem.FeedNode);
				Assert.Equal(node, feedItem.RootNode);
			});

		Assert.Collection(
			viewModel.FooterItems,
			item =>
			{
				Assert.Equal("Settings", item.Title);
				Assert.Equal(Symbol.Settings, item.Symbol);
				Assert.Equal(NavigationRoute.Settings, item.Destination);
				Assert.False(item.IsExpandable);
				Assert.Empty(item.Children);
				Assert.Empty(item.Actions);
			});
	}

	/*[Fact]
	public void UpdatesSelectionWhenPageChanges_ToOtherFeed()
	{
		var viewModel = new MainViewModel(FeedService, ModalService);
		Assert.Equal(viewModel.FeedItems[0], viewModel.SelectedItem);
		var unread = viewModel.FeedItems[1];
		NavigationService.Navigate(unread.Destination);
		Assert.Equal(unread, viewModel.SelectedItem);
	}

	[Fact]
	public void UpdatesSelectionWhenPageChanges_ToSettings()
	{
		var viewModel = new MainViewModel(FeedService, ModalService);
		Assert.Equal(viewModel.FeedItems[0], viewModel.SelectedItem);
		NavigationService.Navigate(NavigationRoute.Settings);
		Assert.Equal(viewModel.FooterItems[0], viewModel.SelectedItem);
	}

	[Fact]
	public void UpdatesSelectionWhenPageChanges_ToUnknownRoute()
	{
		var viewModel = new MainViewModel(FeedService, ModalService);
		Assert.Equal(viewModel.FeedItems[0], viewModel.SelectedItem);
		NavigationService.Navigate(NavigationRoute.Feed(
			FeedNode.Custom(new EmptyFeed(), "Foo", Symbol.Feed, false)));
		Assert.Null(viewModel.SelectedItem);
	}

	[Fact]
	public void UpdatesPageWhenSelectionChanges()
	{
		var viewModel = new MainViewModel(FeedService, ModalService);
		viewModel.SelectedItem = viewModel.FooterItems[0];
		Assert.Equal(viewModel.FooterItems[0], viewModel.SelectedItem);
		Assert.Equal(NavigationRoute.Settings, NavigationService.CurrentRoute);
	}

	[Fact]
	public void UpdatesVisiblePage()
	{
		var viewModel = new MainViewModel(FeedService, ModalService);
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
		NavigationService.Navigate(viewModel.FeedItems[1].Destination);
		Assert.Equal(MainViewModel.Page.Feed, viewModel.VisiblePage);
	}

	[Fact]
	public void GoBack_UpdatesAvailability()
	{
		var viewModel = new MainViewModel(FeedService, ModalService);
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
		var viewModel = new MainViewModel(FeedService, ModalService);
		NavigationService.Navigate(NavigationRoute.Settings);
		viewModel.GoBackCommand.Execute(null);
		Assert.Equal(viewModel.FeedItems[0].Destination, NavigationService.CurrentRoute);
		Assert.False(viewModel.GoBackCommand.CanExecute(null));
	}*/
}
