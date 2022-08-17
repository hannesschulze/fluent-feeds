using System.Collections.ObjectModel;
using FluentFeeds.App.Shared.Models;
using FluentFeeds.Common;

namespace FluentFeeds.App.Shared.ViewModels.Main;

/// <summary>
/// A navigational list item on the main page. 
/// </summary>
public class MainNavigationItemViewModel : MainItemViewModel
{
	public MainNavigationItemViewModel(string title, Symbol symbol, bool isExpandable, NavigationRoute destination) 
		: base(isSelectable: true, isExpandable)
	{
		_title = title;
		_symbol = symbol;
		Destination = destination;
	}

	public override MainItemViewModelType Type => MainItemViewModelType.Navigation;

	/// <summary>
	/// The destination navigation route associated with this item.
	/// </summary>
	public NavigationRoute Destination { get; }

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

	private string _title;
	private Symbol _symbol;
}
