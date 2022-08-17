using System.Collections.ObjectModel;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace FluentFeeds.App.Shared.ViewModels.Main;

/// <summary>
/// A list item on the main page.
/// </summary>
public abstract class MainItemViewModel : ObservableObject
{
	protected MainItemViewModel(bool isSelectable, bool isExpandable)
	{
		IsSelectable = isSelectable;
		IsExpandable = isExpandable;
	}
	
	/// <summary>
	/// The type of this item.
	/// </summary>
	public abstract MainItemViewModelType Type { get; }
	
	/// <summary>
	/// Check whether the item can be selected.
	/// </summary>
	public bool IsSelectable { get; }
	
	/// <summary>
	/// Check whether this item can be expanded to show child elements.
	/// </summary>
	public bool IsExpandable { get; }

	/// <summary>
	/// A list of child elements for this item.
	/// </summary>
	public ObservableCollection<MainItemViewModel> Children { get; } = new();
}
