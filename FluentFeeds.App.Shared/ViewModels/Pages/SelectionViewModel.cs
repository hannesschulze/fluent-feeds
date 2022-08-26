using System;
using FluentFeeds.App.Shared.Models.Navigation;
using FluentFeeds.App.Shared.Resources;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace FluentFeeds.App.Shared.ViewModels.Pages;

/// <summary>
/// View model for a placeholder page displaying information about the currently selected items.
/// </summary>
public sealed class SelectionViewModel : ObservableObject
{
	public SelectionViewModel()
	{
		_title = LocalizedStrings.SelectionTitleNoItems;
		_isMessageVisible = true;
	}

	/// <summary>
	/// Called after navigating to the selection page.
	/// </summary>
	/// <param name="route">Route containing parameters.</param>
	public void Load(FeedNavigationRoute route)
	{
		if (route.Type != FeedNavigationRouteType.Selection)
			throw new Exception("Invalid route type.");

		Title = route.SelectionCount == 0
			? LocalizedStrings.SelectionTitleNoItems
			: String.Format(LocalizedStrings.SelectionTitleMultipleItems, route.SelectionCount);
		IsMessageVisible = route.SelectionCount == 0;
	}

	/// <summary>
	/// Title text, always visible.
	/// </summary>
	public string Title
	{
		get => _title;
		private set => SetProperty(ref _title, value);
	}

	/// <summary>
	/// Info text displayed below the title if <see cref="IsMessageVisible"/> is true.
	/// </summary>
	public string Message => LocalizedStrings.SelectionMessage;

	/// <summary>
	/// Flag indicating if <see cref="Message"/> should be shown to the user.
	/// </summary>
	public bool IsMessageVisible
	{
		get => _isMessageVisible;
		private set => SetProperty(ref _isMessageVisible, value);
	}

	private string _title;
	private bool _isMessageVisible;
}
