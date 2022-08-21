using FluentFeeds.Feeds.Base.Nodes;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace FluentFeeds.App.Shared.ViewModels.Items.GroupSelection;

/// <summary>
/// <para>View model for an item in a group node selection menu.</para>
/// 
/// <para>This is used for selecting the parent group node when adding feeds, groups or moving nodes.</para>
/// </summary>
public sealed class GroupSelectionItemViewModel : ObservableObject
{
	public GroupSelectionItemViewModel(IReadOnlyStoredFeedNode feedNode, int indentationLevel, bool isSelectable)
	{
		FeedNode = feedNode;
		Title = FeedNode.ActualTitle ?? "Unnamed";
		IndentationLevel = indentationLevel;
		IsSelectable = isSelectable;
	}

	/// <summary>
	/// The node wrapped by this view model.
	/// </summary>
	public IReadOnlyStoredFeedNode FeedNode { get; }

	/// <summary>
	/// Title of the feed node.
	/// </summary>
	public string Title { get; }

	/// <summary>
	/// Indentation level of the item to display a tree structure.
	/// </summary>
	public int IndentationLevel { get; }

	/// <summary>
	/// Flag indicating if the user can select this item in the menu.
	/// </summary>
	public bool IsSelectable { get; }
}
