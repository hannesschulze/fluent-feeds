using FluentFeeds.App.Shared.Models.Feeds;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace FluentFeeds.App.Shared.ViewModels.Items.GroupSelection;

/// <summary>
/// <para>View model for an item in a group selection menu.</para>
/// 
/// <para>This is used for selecting the parent group when adding feeds, groups or moving feeds.</para>
/// </summary>
public sealed class GroupSelectionItemViewModel : ObservableObject
{
	public GroupSelectionItemViewModel(IFeedView feed, int indentationLevel, bool isSelectable)
	{
		Feed = feed;
		Title = Feed.DisplayName;
		IndentationLevel = indentationLevel;
		IsSelectable = isSelectable;
	}

	/// <summary>
	/// The feed wrapped by this view model.
	/// </summary>
	public IFeedView Feed { get; }

	/// <summary>
	/// Title of the feed.
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
