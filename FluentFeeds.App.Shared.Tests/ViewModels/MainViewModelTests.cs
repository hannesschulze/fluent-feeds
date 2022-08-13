using FluentFeeds.App.Shared.Models;
using FluentFeeds.App.Shared.Services.Default;
using FluentFeeds.App.Shared.Tests.Services.Mock;
using FluentFeeds.App.Shared.ViewModels;
using FluentFeeds.Common;
using FluentFeeds.Feeds.Base;
using FluentFeeds.Feeds.Base.Nodes;
using Xunit;

namespace FluentFeeds.App.Shared.Tests.ViewModels;

public class MainViewModelTests
{
	public FeedServiceMock FeedService { get; }
	public NavigationService NavigationService { get; }

	public MainViewModelTests()
	{
		FeedService = new FeedServiceMock();
		NavigationService = new NavigationService(FeedService);
	}

	[Fact]
	public void HasDefaultNavigationItems()
	{
		var viewModel = new MainViewModel(NavigationService);
		Assert.Equal(2, viewModel.FeedItems.Count);

		var overview = viewModel.FeedItems[0];
		Assert.Equal("Overview", overview.Title);
		Assert.Equal(Symbol.Home, overview.Symbol);
		Assert.Equal(NavigationRouteType.Feed, overview.Destination?.Type);
		Assert.Equal(NavigationService.CurrentRoute, overview.Destination);
		Assert.False(overview.IsExpandable);
		Assert.True(overview.IsSelectable);
		Assert.Empty(overview.Children);

		var unread = viewModel.FeedItems[1];
		Assert.Equal("Unread", unread.Title);
		Assert.Equal(Symbol.Sparkle, unread.Symbol);
		Assert.Equal(NavigationRouteType.Feed, unread.Destination?.Type);
		Assert.False(unread.IsExpandable);
		Assert.True(unread.IsSelectable);
		Assert.Empty(unread.Children);

		var settings = viewModel.SettingsItem;
		Assert.Equal("Settings", settings.Title);
		Assert.Equal(Symbol.Settings, settings.Symbol);
		Assert.Equal(NavigationRoute.Settings, settings.Destination);
		Assert.False(settings.IsExpandable);
		Assert.True(settings.IsSelectable);
		Assert.Empty(settings.Children);
	}

	[Fact]
	public void UpdatesSelectionWhenPageChanges_ToOtherFeed()
	{
		var viewModel = new MainViewModel(NavigationService);
		Assert.Equal(viewModel.FeedItems[0], viewModel.SelectedItem);
		var unread = viewModel.FeedItems[1];
		NavigationService.Navigate(unread.Destination!.Value);
		Assert.Equal(unread, viewModel.SelectedItem);
	}

	[Fact]
	public void UpdatesSelectionWhenPageChanges_ToSettings()
	{
		var viewModel = new MainViewModel(NavigationService);
		Assert.Equal(viewModel.FeedItems[0], viewModel.SelectedItem);
		NavigationService.Navigate(NavigationRoute.Settings);
		Assert.Equal(viewModel.SettingsItem, viewModel.SelectedItem);
	}

	[Fact]
	public void UpdatesSelectionWhenPageChanges_ToUnknownRoute()
	{
		var viewModel = new MainViewModel(NavigationService);
		Assert.Equal(viewModel.FeedItems[0], viewModel.SelectedItem);
		NavigationService.Navigate(NavigationRoute.Feed(
			FeedNode.Custom(new EmptyFeed(), "Foo", Symbol.Feed, false)));
		Assert.Null(viewModel.SelectedItem);
	}

	[Fact]
	public void UpdatesPageWhenSelectionChanges()
	{
		var viewModel = new MainViewModel(NavigationService);
		viewModel.SelectedItem = viewModel.SettingsItem;
		Assert.Equal(viewModel.SettingsItem, viewModel.SelectedItem);
		Assert.Equal(NavigationRoute.Settings, NavigationService.CurrentRoute);
	}

	[Fact]
	public void UpdatesVisiblePage()
	{
		var viewModel = new MainViewModel(NavigationService);
		Assert.Equal(MainViewModel.Page.Feed, viewModel.VisiblePage);
		NavigationService.Navigate(NavigationRoute.Settings);
		Assert.Equal(MainViewModel.Page.Settings, viewModel.VisiblePage);
	}

	[Fact]
	public void UpdatesVisiblePage_OnlyForDifferentRouteType()
	{
		var viewModel = new MainViewModel(NavigationService);
		viewModel.PropertyChanged +=
			(s, e) => Assert.NotEqual(nameof(MainViewModel.VisiblePage), e.PropertyName);
		NavigationService.Navigate(viewModel.FeedItems[1].Destination!.Value);
		Assert.Equal(MainViewModel.Page.Feed, viewModel.VisiblePage);
	}

	[Fact]
	public void GoBack_UpdatesAvailability()
	{
		var viewModel = new MainViewModel(NavigationService);
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
		var viewModel = new MainViewModel(NavigationService);
		NavigationService.Navigate(NavigationRoute.Settings);
		viewModel.GoBackCommand.Execute(null);
		Assert.Equal(viewModel.FeedItems[0].Destination, NavigationService.CurrentRoute);
		Assert.False(viewModel.GoBackCommand.CanExecute(null));
	}
}
