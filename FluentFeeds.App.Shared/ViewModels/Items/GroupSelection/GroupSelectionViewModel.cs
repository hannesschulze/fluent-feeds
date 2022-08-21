using System.Collections.ObjectModel;
using AngleSharp.Dom;
using FluentFeeds.App.Shared.Models;
using FluentFeeds.Feeds.Base;
using FluentFeeds.Feeds.Base.Nodes;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace FluentFeeds.App.Shared.ViewModels.Items.GroupSelection;

/// <summary>
/// View model for selecting a group node in a feed provider from a menu.
/// </summary>
public sealed class GroupSelectionViewModel : ObservableObject
{
	/// <summary>
	/// Create a new group selection view model.
	/// </summary>
	/// <param name="provider">The provider whose items should be shown in the menu.</param>
	/// <param name="selectedNode">The initial selection.</param>
	/// <param name="forbiddenNode">A node which cannot be selected.</param>
	public GroupSelectionViewModel(
		LoadedFeedProvider? provider, IReadOnlyFeedNode? selectedNode, IReadOnlyFeedNode? forbiddenNode)
	{
		if (provider?.RootNode is IReadOnlyStoredFeedNode storedRootNode)
		{
			AddItem(storedRootNode, selectedNode, forbiddenNode);
		}

		Items = new ReadOnlyObservableCollection<GroupSelectionItemViewModel>(_items);
	}

	private void AddItem(
		IReadOnlyStoredFeedNode node, IReadOnlyFeedNode? selectedNode, IReadOnlyFeedNode? forbiddenNode,
		bool isInForbiddenItem = false, int indentationLevel = 0)
	{
		isInForbiddenItem = isInForbiddenItem || node == forbiddenNode;
		var item = new GroupSelectionItemViewModel(
			node, indentationLevel,
			isSelectable: !isInForbiddenItem && node.IsUserCustomizable && node.Type == FeedNodeType.Group);
		if (node == selectedNode)
			_selectedItem = item;
		_items.Add(item);

		// Add children right after this item.
		if (node.Children != null)
		{
			foreach (var child in node.Children)
			{
				if (child is IReadOnlyStoredFeedNode storedChild)
				{
					AddItem(storedChild, selectedNode, forbiddenNode, isInForbiddenItem, indentationLevel + 1);
				}
			}
		}
	}

	/// <summary>
	/// Available items which can be used to select the group.
	/// </summary>
	public ReadOnlyObservableCollection<GroupSelectionItemViewModel> Items { get; }

	/// <summary>
	/// The currently selected item.
	/// </summary>
	public GroupSelectionItemViewModel? SelectedItem
	{
		get => _selectedItem;
		set => SetProperty(ref _selectedItem, value != null && value.IsSelectable ? value : null);
	}

	private GroupSelectionItemViewModel? _selectedItem;
	private readonly ObservableCollection<GroupSelectionItemViewModel> _items = new();
}
