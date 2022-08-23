using System;
using FluentFeeds.App.Shared.Models.Navigation;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace FluentFeeds.App.Shared.ViewModels.Pages;

/// <summary>
/// View model for a placeholder page displaying information about the currently selected items.
/// </summary>
public sealed class SelectionViewModel : ObservableObject
{
	public SelectionViewModel()
	{
		_title = "No items selected";
		_isInfoTextVisible = true;
		InfoText = "The content of the selected item will be shown here.";
	}

	/// <summary>
	/// Called after navigating to the selection page.
	/// </summary>
	/// <param name="route">Route containing parameters.</param>
	public void Load(FeedNavigationRoute route)
	{
		if (route.Type != FeedNavigationRouteType.Selection)
			throw new Exception("Invalid route type.");

		Title = route.SelectionCount == 0 ? "No items selected" : $"{route.SelectionCount} items selected";
		IsInfoTextVisible = route.SelectionCount == 0;
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
	/// Info text displayed below the title if <see cref="IsInfoTextVisible"/> is true.
	/// </summary>
	public string InfoText { get; }

	/// <summary>
	/// Flag indicating if <see cref="InfoText"/> should be shown to the user.
	/// </summary>
	public bool IsInfoTextVisible
	{
		get => _isInfoTextVisible;
		private set => SetProperty(ref _isInfoTextVisible, value);
	}

	private string _title;
	private bool _isInfoTextVisible;
}
