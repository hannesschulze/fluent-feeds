using System.Collections.ObjectModel;
using FluentFeeds.App.Shared.Models;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace FluentFeeds.App.Shared.ViewModels;

/// <summary>
/// View model for an item in the app navigation.
/// </summary>
public abstract class NavigationItemViewModel : ObservableObject
{
	protected NavigationItemViewModel(
		string title, Symbol symbol, bool isExpandable, NavigationRoute? destination)
	{
		_title = title;
		_symbol = symbol;
		IsExpandable = isExpandable;
		Destination = destination;
		Children = new(MutableChildren);
	}

	/// <summary>
	/// Check whether this item can be expanded to show child elements.
	/// </summary>
	public bool IsExpandable { get; }

	/// <summary>
	/// Check whether this item can be selected, i.e. if it has a navigation route associated with it.
	/// </summary>
	public bool IsSelectable => Destination != null;

	/// <summary>
	/// The destination navigation route associated with this item (if it is selectable).
	/// </summary>
	public NavigationRoute? Destination { get; }

	/// <summary>
	/// Human-readable text displayed for the item.
	/// </summary>
	public string Title
	{
		get => _title;
		protected set => SetProperty(ref _title, value);
	}

	/// <summary>
	/// Symbol representing this item.
	/// </summary>
	public Symbol Symbol
	{
		get => _symbol;
		protected set => SetProperty(ref _symbol, value);
	}

	/// <summary>
	/// A list of child elements for this item.
	/// </summary>
	public ReadOnlyObservableCollection<NavigationItemViewModel> Children { get; }

	protected ObservableCollection<NavigationItemViewModel> MutableChildren { get; } = new();

	private string _title;
	private Symbol _symbol;
}
