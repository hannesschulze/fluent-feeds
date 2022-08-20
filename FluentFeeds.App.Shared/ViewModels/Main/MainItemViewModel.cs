using System.Collections.Immutable;
using System.Collections.ObjectModel;
using FluentFeeds.App.Shared.Models;
using FluentFeeds.Common;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace FluentFeeds.App.Shared.ViewModels.Main;

/// <summary>
/// A list item on the main page.
/// </summary>
public class MainItemViewModel : ObservableObject
{
	public MainItemViewModel(
		NavigationRoute destination, bool isExpandable, string title, Symbol symbol,
		ImmutableArray<MainItemActionViewModel>? actions = null)
	{
		Destination = destination;
		IsExpandable = isExpandable;
		_title = title;
		_symbol = symbol;
		_actions = actions;
	}

	/// <summary>
	/// The destination navigation route associated with this item.
	/// </summary>
	public NavigationRoute Destination { get; }

	/// <summary>
	/// Check whether this item can be expanded to show child elements.
	/// </summary>
	public bool IsExpandable { get; }

	/// <summary>
	/// Human-readable text displayed for the item.
	/// </summary>
	public string Title
	{
		get => _title;
		set => SetProperty(ref _title, value);
	}

	/// <summary>
	/// Symbol representing this item.
	/// </summary>
	public Symbol Symbol
	{
		get => _symbol;
		set => SetProperty(ref _symbol, value);
	}

	/// <summary>
	/// A list of possible actions which can be executed on this item.
	/// </summary>
	public ImmutableArray<MainItemActionViewModel>? Actions
	{
		get => _actions;
		set => SetProperty(ref _actions, value);
	}

	/// <summary>
	/// A list of child elements for this item.
	/// </summary>
	public ObservableCollection<MainItemViewModel> Children { get; } = new();

	private string _title;
	private Symbol _symbol;
	private ImmutableArray<MainItemActionViewModel>? _actions;
}
