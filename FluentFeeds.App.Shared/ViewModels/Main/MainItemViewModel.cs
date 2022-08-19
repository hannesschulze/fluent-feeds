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
	/// Flag indicating if this item can be dragged by the user.
	/// </summary>
	public virtual bool CanDrag => false;

	/// <summary>
	/// Check if <c>otherItem</c> can be dropped onto this item by the user.
	/// </summary>
	/// <param name="otherItem"></param>
	/// <returns></returns>
	public virtual bool CanDrop(MainItemViewModel otherItem) => false;

	/// <summary>
	/// Handle <c>otherItem</c> being dropped onto this item by the user.
	/// </summary>
	/// <param name="otherItem"></param>
	public virtual void HandleDrop(MainItemViewModel otherItem)
	{
	}

	/// <summary>
	/// A list of possible actions which can be executed on this item.
	/// </summary>
	public ObservableCollection<MainItemActionViewModel> Actions { get; } = new();

	/// <summary>
	/// A list of child elements for this item.
	/// </summary>
	public ObservableCollection<MainItemViewModel> Children { get; } = new();
}
