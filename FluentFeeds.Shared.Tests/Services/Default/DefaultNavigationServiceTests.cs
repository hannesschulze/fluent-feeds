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
		Assert.Equal(Service.BackStack[0], Service.CurrentEntry);
		Assert.False(Service.CanGoBack);
	}

	[Fact]
	public void Navigate_ToNewEntry()
	{
		Assert.Raises<EventArgs>(
			h => Service.BackStackChanged += h,
			h => Service.BackStackChanged -= h,
			() => Service.Navigate(NavigationEntry.Settings));
		Assert.Equal(2, Service.BackStack.Count);
		Assert.Equal(NavigationEntry.Settings, Service.CurrentEntry);
		Assert.True(Service.CanGoBack);
	}

	[Fact]
	public void Navigate_ToSameEntry()
	{
		Service.Navigate(NavigationEntry.Settings);
		var changed = false;
		Service.BackStackChanged += (s, e) => changed = true;
		Service.Navigate(NavigationEntry.Settings);

		Assert.False(changed);
		Assert.Equal(2, Service.BackStack.Count);
		Assert.Equal(NavigationEntry.Settings, Service.CurrentEntry);
		Assert.True(Service.CanGoBack);
	}

	[Fact]
	public void GoBack_WithFullBackStack()
	{
		var firstEntry = Service.CurrentEntry;
		Service.Navigate(NavigationEntry.Settings);

		Assert.Raises<EventArgs>(
			h => Service.BackStackChanged += h,
			h => Service.BackStackChanged -= h,
			() => Service.GoBack());
		Assert.Equal(1, Service.BackStack.Count);
		Assert.Equal(firstEntry, Service.CurrentEntry);
		Assert.False(Service.CanGoBack);
	}

	[Fact]
	public void GoBack_WithEmptyBackStack()
	{
		var firstEntry = Service.CurrentEntry;
		var changed = false;
		Service.BackStackChanged += (s, e) => changed = true;
		Service.GoBack();

		Assert.False(changed);
		Assert.Equal(1, Service.BackStack.Count);
		Assert.Equal(firstEntry, Service.CurrentEntry);
		Assert.False(Service.CanGoBack);
	}
}
