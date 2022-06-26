using System;
using FluentFeeds.Shared.Models;
using FluentFeeds.Shared.Services.Default;
using Xunit;

namespace FluentFeeds.Shared.Tests.Services.Default;

public class DefaultNavigationServiceTests
{
	public DefaultNavigationService Service { get; } = new();

	[Fact]
	public void InitialBackStackIsNotEmpty()
	{
		Assert.Equal(1, Service.BackStack.Count);
		Assert.Equal(Service.BackStack[0], Service.CurrentRoute);
		Assert.False(Service.CanGoBack);
	}

	[Fact]
	public void Navigate_ToNewEntry()
	{
		Assert.Raises<EventArgs>(
			h => Service.BackStackChanged += h,
			h => Service.BackStackChanged -= h,
			() => Service.Navigate(NavigationRoute.Settings));
		Assert.Equal(2, Service.BackStack.Count);
		Assert.Equal(NavigationRoute.Settings, Service.CurrentRoute);
		Assert.True(Service.CanGoBack);
	}

	[Fact]
	public void Navigate_ToSameEntry()
	{
		Service.Navigate(NavigationRoute.Settings);
		var changed = false;
		Service.BackStackChanged += (s, e) => changed = true;
		Service.Navigate(NavigationRoute.Settings);

		Assert.False(changed);
		Assert.Equal(2, Service.BackStack.Count);
		Assert.Equal(NavigationRoute.Settings, Service.CurrentRoute);
		Assert.True(Service.CanGoBack);
	}

	[Fact]
	public void GoBack_WithFullBackStack()
	{
		var firstEntry = Service.CurrentRoute;
		Service.Navigate(NavigationRoute.Settings);

		Assert.Raises<EventArgs>(
			h => Service.BackStackChanged += h,
			h => Service.BackStackChanged -= h,
			() => Service.GoBack());
		Assert.Equal(1, Service.BackStack.Count);
		Assert.Equal(firstEntry, Service.CurrentRoute);
		Assert.False(Service.CanGoBack);
	}

	[Fact]
	public void GoBack_WithEmptyBackStack()
	{
		var firstEntry = Service.CurrentRoute;
		var changed = false;
		Service.BackStackChanged += (s, e) => changed = true;
		Service.GoBack();

		Assert.False(changed);
		Assert.Equal(1, Service.BackStack.Count);
		Assert.Equal(firstEntry, Service.CurrentRoute);
		Assert.False(Service.CanGoBack);
	}
}
