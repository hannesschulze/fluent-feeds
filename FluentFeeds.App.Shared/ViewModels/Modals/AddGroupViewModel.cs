using System;
using System.Threading.Tasks;
using FluentFeeds.Common;
using FluentFeeds.Feeds.Base.Nodes;

namespace FluentFeeds.App.Shared.ViewModels.Modals;

/// <summary>
/// View model for a modal allowing the user to add a child group to the selected group.
/// </summary>
public sealed class AddGroupViewModel : NodeDataViewModel
{
	public AddGroupViewModel(IReadOnlyStoredFeedNode rootNode, IReadOnlyFeedNode? parentGroup)
		: base(
			title: "Add a group", errorTitle: "Unable to create the group",
			errorMessage: "An error occurred while trying to create the group.", inputLabel: "Name",
			showProgressSpinner: false, rootNode, parentGroup, null)
	{
	}

	protected override async Task SaveAsync(IReadOnlyStoredFeedNode selectedGroup)
	{
		var storage = selectedGroup.Storage;
		var node = FeedNode.Group(Input.Trim(), Symbol.Directory, isUserCustomizable: true);
		await storage.AddNodeAsync(node, selectedGroup.Identifier);
	}

	protected override void HandleInputChanged()
	{
		IsInputValid = !String.IsNullOrWhiteSpace(Input);
	}
}
