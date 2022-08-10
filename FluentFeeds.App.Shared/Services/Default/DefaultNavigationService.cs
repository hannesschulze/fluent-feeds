﻿using System;
using System.Collections.Generic;
using FluentFeeds.App.Shared.Models;
using FluentFeeds.Common;
using FluentFeeds.Feeds.Base;
using FluentFeeds.Feeds.Base.Nodes;

namespace FluentFeeds.App.Shared.Services.Default;

/// <summary>
/// Default implementation of the navigation service interface.
/// </summary>
public class DefaultNavigationService : INavigationService
{
	public DefaultNavigationService()
	{
		_backStack.Add(NavigationRoute.Feed(FeedNode.Custom(new EmptyFeed(), "Overview", Symbol.Home, false)));
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