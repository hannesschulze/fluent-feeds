using System;
using System.Collections.Generic;
using FluentFeeds.Shared.Models;

namespace FluentFeeds.Shared.Services.Default;

/// <summary>
/// Default implementation of the navigation service interface.
/// </summary>
public class DefaultNavigationService : INavigationService
{
	public DefaultNavigationService()
	{
		_backStack.Add(NavigationEntry.FeedItem(new Feed(), null));
	}

	public event EventHandler<EventArgs>? BackStackChanged;

	protected virtual void OnBackStackChanged() => BackStackChanged?.Invoke(this, EventArgs.Empty);

	public IReadOnlyList<NavigationEntry> BackStack => _backStack;
	public NavigationEntry CurrentEntry => BackStack[BackStack.Count - 1];
	public bool CanGoBack => BackStack.Count > 1;

	public void GoBack()
	{
		if (!CanGoBack)
			return;

		_backStack.RemoveAt(BackStack.Count - 1);
		OnBackStackChanged();
	}

	public void Navigate(NavigationEntry destination)
	{
		if (destination == CurrentEntry)
			return;

		_backStack.Add(destination);
		OnBackStackChanged();
	}

	private List<NavigationEntry> _backStack = new();
}
