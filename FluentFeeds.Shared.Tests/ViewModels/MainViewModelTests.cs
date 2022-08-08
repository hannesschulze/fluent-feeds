using System.ComponentModel;
using FluentFeeds.Shared.Models;
using FluentFeeds.Shared.Models.Feeds;
using FluentFeeds.Shared.Models.Nodes;
using FluentFeeds.Shared.Services;
using FluentFeeds.Shared.Services.Default;
using FluentFeeds.Shared.ViewModels;
using Xunit;

namespace FluentFeeds.Shared.Tests.ViewModels;

public class MainViewModelTests
{
	public INavigationService NavigationService { get; }
	public MainViewModel ViewModel { get; }

	public MainViewModelTests()
	{
		NavigationService = new DefaultNavigationService();
		ViewModel = new MainViewModel(NavigationService);
	}

	[Fact]
	public void HasDefaultNavigationItems()
	{
		Assert.Equal(2, ViewModel.FeedItems.Count);

		var overview = ViewModel.FeedItems[0];
		Assert.Equal("Overview", overview.Title);
		Assert.Equal(Symbol.Home, overview.Symbol);
		Assert.Equal(NavigationRouteType.Feed, overview.Destination?.Type);
		Assert.Equal(NavigationService.CurrentRoute, overview.Destination);
		Assert.False(overview.IsExpandable);
		Assert.True(overview.IsSelectable);
		Assert.Empty(overview.Children);

		var unread = ViewModel.FeedItems[1];
		Assert.Equal("Unread", unread.Title);
		Assert.Equal(Symbol.Sparkle, unread.Symbol);
		Assert.Equal(NavigationRouteType.Feed, unread.Destination?.Type);
		Assert.False(unread.IsExpandable);
		Assert.True(unread.IsSelectable);
		Assert.Empty(unread.Children);

		var settings = ViewModel.SettingsItem;
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
		Assert.Equal(ViewModel.FeedItems[0], ViewModel.SelectedItem);
		var unread = ViewModel.FeedItems[1];
		NavigationService.Navigate(unread.Destination!.Value);
		Assert.Equal(unread, ViewModel.SelectedItem);
	}

	[Fact]
	public void UpdatesSelectionWhenPageChanges_ToSettings()
	{
		Assert.Equal(ViewModel.FeedItems[0], ViewModel.SelectedItem);
		NavigationService.Navigate(NavigationRoute.Settings);
		Assert.Equal(ViewModel.SettingsItem, ViewModel.SelectedItem);
	}

	[Fact]
	public void UpdatesSelectionWhenPageChanges_ToUnknownRoute()
	{
		Assert.Equal(ViewModel.FeedItems[0], ViewModel.SelectedItem);
		NavigationService.Navigate(NavigationRoute.Feed(new FeedItem("Foo", Symbol.Rss, new EmptyFeed())));
		Assert.Null(ViewModel.SelectedItem);
	}

	[Fact]
	public void UpdatesPageWhenSelectionChanges()
	{
		ViewModel.SelectedItem = ViewModel.SettingsItem;
		Assert.Equal(ViewModel.SettingsItem, ViewModel.SelectedItem);
		Assert.Equal(NavigationRoute.Settings, NavigationService.CurrentRoute);
	}

	[Fact]
	public void UpdatesVisiblePage()
	{
		Assert.Equal(MainViewModel.Page.Feed, ViewModel.VisiblePage);
		NavigationService.Navigate(NavigationRoute.Settings);
		Assert.Equal(MainViewModel.Page.Settings, ViewModel.VisiblePage);
	}

	[Fact]
	public void UpdatesVisiblePage_OnlyForDifferentRouteType()
	{
		ViewModel.PropertyChanged +=
			(s, e) => Assert.NotEqual(nameof(MainViewModel.VisiblePage), e.PropertyName);
		NavigationService.Navigate(ViewModel.FeedItems[1].Destination!.Value);
		Assert.Equal(MainViewModel.Page.Feed, ViewModel.VisiblePage);
	}

	[Fact]
	public void GoBack_UpdatesAvailability()
	{
		Assert.False(ViewModel.GoBackCommand.CanExecute(null));
		var changed = false;
		ViewModel.GoBackCommand.CanExecuteChanged += (s, e) => changed = true;
		NavigationService.Navigate(NavigationRoute.Settings);
		Assert.True(changed);
		Assert.True(ViewModel.GoBackCommand.CanExecute(null));
	}

	[Fact]
	public void GoBack_ExecuteCommand()
	{
		NavigationService.Navigate(NavigationRoute.Settings);
		ViewModel.GoBackCommand.Execute(null);
		Assert.Equal(ViewModel.FeedItems[0].Destination, NavigationService.CurrentRoute);
		Assert.False(ViewModel.GoBackCommand.CanExecute(null));
	}
}
