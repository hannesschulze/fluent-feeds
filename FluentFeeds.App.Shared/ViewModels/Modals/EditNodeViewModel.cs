using System;
using System.Threading.Tasks;
using FluentFeeds.Feeds.Base.Nodes;

namespace FluentFeeds.App.Shared.ViewModels.Modals;

/// <summary>
/// View model for a modal allowing the user to rename or move a tree node.
/// </summary>
public sealed class EditNodeViewModel : NodeDataViewModel
{
	public EditNodeViewModel(IReadOnlyStoredFeedNode rootNode, IReadOnlyStoredFeedNode node)
		: base(
			title: "Edit an item", errorTitle: "Unable to edit the item",
			errorMessage: "An error occurred while trying to apply changes to the item.", inputLabel: "Name",
			showProgressSpinner: false, rootNode, node.Storage.GetNodeParent(node.Identifier), node)
	{
		_node = node;
		Input = node.DisplayTitle;
	}

	protected override async Task SaveAsync(IReadOnlyStoredFeedNode selectedGroup)
	{
		var storage = _node.Storage;
		
		var newTitle = Input.Trim();
		if (newTitle != _node.DisplayTitle)
		{
			await storage.RenameNodeAsync(_node.Identifier, newTitle);
		}

		await storage.MoveNodeAsync(_node.Identifier, selectedGroup.Identifier);
	}

	protected override void HandleInputChanged()
	{
		IsInputValid = !String.IsNullOrWhiteSpace(Input);
	}

	private readonly IReadOnlyStoredFeedNode _node;
}
