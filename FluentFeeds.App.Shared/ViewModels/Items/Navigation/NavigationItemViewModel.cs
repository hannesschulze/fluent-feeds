using System.Collections.Immutable;
using System.Collections.ObjectModel;
using FluentFeeds.App.Shared.Models;
using FluentFeeds.Common;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace FluentFeeds.App.Shared.ViewModels.Items.Navigation;

/// <summary>
/// A navigation item on the main page.
/// </summary>
public class NavigationItemViewModel : ObservableObject
{
	public NavigationItemViewModel(NavigationRoute destination, bool isExpandable, string title, Symbol symbol)
	{
		Destination = destination;
		IsExpandable = isExpandable;
		Children = new ReadOnlyObservableCollection<NavigationItemViewModel>(MutableChildren);
		_title = title;
		_symbol = symbol;
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
	/// A list of possible actions which can be executed on this item.
	/// </summary>
	public ImmutableArray<NavigationItemActionViewModel>? Actions
	{
		get => _actions;
		protected set => SetProperty(ref _actions, value);
	}

	/// <summary>
	/// A list of child elements for this item.
	/// </summary>
	public ReadOnlyObservableCollection<NavigationItemViewModel> Children { get; }

	/// <summary>
	/// Mutable list of child elements.
	/// </summary>
	protected ObservableCollection<NavigationItemViewModel> MutableChildren { get; } = new();

	private string _title;
	private Symbol _symbol;
	private ImmutableArray<NavigationItemActionViewModel>? _actions;
}
