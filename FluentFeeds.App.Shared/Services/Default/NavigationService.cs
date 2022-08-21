using System;
using System.Collections.Generic;
using FluentFeeds.App.Shared.Models;

namespace FluentFeeds.App.Shared.Services.Default;

public class NavigationService : INavigationService
{
	public NavigationService(IFeedService feedService)
	{
		_backStack.Add(NavigationRoute.Feed(feedService.OverviewNode));
	}

	public event EventHandler<EventArgs>? BackStackChanged;

	protected virtual void OnBackStackChanged() => BackStackChanged?.Invoke(this, EventArgs.Empty);

	public IReadOnlyList<NavigationRoute> BackStack => _backStack;
	public NavigationRoute CurrentRoute => BackStack[^1];
	public bool CanGoBack => BackStack.Count > 1;

	public void GoBack()
	{
		if (!CanGoBack)
			return;

		_backStack.RemoveAt(BackStack.Count - 1);
		OnBackStackChanged();
	}

	public void Navigate(NavigationRoute destination)
	{
		if (destination == CurrentRoute)
			return;

		_backStack.Add(destination);
		OnBackStackChanged();
	}

	private readonly List<NavigationRoute> _backStack = new();
}
