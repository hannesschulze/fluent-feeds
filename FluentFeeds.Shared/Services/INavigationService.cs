using System;
using System.Collections.Generic;
using FluentFeeds.Shared.Models;

namespace FluentFeeds.Shared.Services;

/// <summary>
/// Service responsible for managing the navigation stack of the application (i.e. switching between feeds and other 
/// routes).
/// </summary>
public interface INavigationService
{
	/// <summary>
	/// A change has been made to the back stack - this might affect the current route and whether the user can go
	/// back.
	/// </summary>
	event EventHandler<EventArgs>? BackStackChanged;

	/// <summary>
	/// The current back stack. This list is never empty.
	/// </summary>
	IReadOnlyList<NavigationRoute> BackStack { get; }

	/// <summary>
	/// The last element of the back stack.
	/// </summary>
	NavigationRoute CurrentRoute { get; }

	/// <summary>
	/// Check whether the user can go back to the previous route. There is at least one route on the back stack at all
	/// times.
	/// </summary>
	bool CanGoBack { get; }

	/// <summary>
	/// Go back to the previous route on the back stack (removing the last element).
	/// </summary>
	void GoBack();

	/// <summary>
	/// Add a new route onto the back stack.
	/// </summary>
	void Navigate(NavigationRoute destination);
}
