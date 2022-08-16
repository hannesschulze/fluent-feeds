using System;
using FluentFeeds.App.Shared.Models;
using FluentFeeds.App.Shared.Services.Default;
using FluentFeeds.App.Shared.Tests.Services.Mock;
using Xunit;

namespace FluentFeeds.App.Shared.Tests.Services;

public class DefaultNavigationServiceTests
{
	private FeedServiceMock FeedService { get; } = new();

	[Fact]
	public void InitialBackStackIsNotEmpty()
	{
		var service = new NavigationService(FeedService);
		Assert.Equal(1, service.BackStack.Count);
		Assert.Equal(service.BackStack[0], service.CurrentRoute);
		Assert.Equal(NavigationRoute.Feed(FeedService.OverviewFeed), service.CurrentRoute);
		Assert.False(service.CanGoBack);
	}

	[Fact]
	public void Navigate_ToNewEntry()
	{
		var service = new NavigationService(FeedService);
		Assert.Raises<EventArgs>(
			h => service.BackStackChanged += h,
			h => service.BackStackChanged -= h,
			() => service.Navigate(NavigationRoute.Settings));
		Assert.Equal(2, service.BackStack.Count);
		Assert.Equal(NavigationRoute.Settings, service.CurrentRoute);
		Assert.True(service.CanGoBack);
	}

	[Fact]
	public void Navigate_ToSameEntry()
	{
		var service = new NavigationService(FeedService);
		service.Navigate(NavigationRoute.Settings);
		var changed = false;
		service.BackStackChanged += (s, e) => changed = true;
		service.Navigate(NavigationRoute.Settings);

		Assert.False(changed);
		Assert.Equal(2, service.BackStack.Count);
		Assert.Equal(NavigationRoute.Settings, service.CurrentRoute);
		Assert.True(service.CanGoBack);
	}

	[Fact]
	public void GoBack_WithFullBackStack()
	{
		var service = new NavigationService(FeedService);
		service.Navigate(NavigationRoute.Settings);

		Assert.Raises<EventArgs>(
			h => service.BackStackChanged += h,
			h => service.BackStackChanged -= h,
			() => service.GoBack());
		Assert.Equal(1, service.BackStack.Count);
		Assert.Equal(NavigationRoute.Feed(FeedService.OverviewFeed), service.CurrentRoute);
		Assert.False(service.CanGoBack);
	}

	[Fact]
	public void GoBack_WithEmptyBackStack()
	{
		var service = new NavigationService(FeedService);
		var changed = false;
		service.BackStackChanged += (s, e) => changed = true;
		service.GoBack();

		Assert.False(changed);
		Assert.Equal(1, service.BackStack.Count);
		Assert.Equal(NavigationRoute.Feed(FeedService.OverviewFeed), service.CurrentRoute);
		Assert.False(service.CanGoBack);
	}
}
