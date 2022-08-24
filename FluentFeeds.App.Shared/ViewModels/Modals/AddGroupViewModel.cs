using System;
using System.Threading.Tasks;
using FluentFeeds.App.Shared.Models.Feeds;
using FluentFeeds.App.Shared.Models.Storage;
using FluentFeeds.Common;
using FluentFeeds.Feeds.Base.Feeds;

namespace FluentFeeds.App.Shared.ViewModels.Modals;

/// <summary>
/// View model for a modal allowing the user to add a child group to the selected group.
/// </summary>
public sealed class AddGroupViewModel : FeedDataViewModel
{
	public AddGroupViewModel(IFeedView rootFeed, IFeedView? parentGroup, IFeedStorage storage)
		: base(
			title: "Add a group", errorTitle: "Unable to create the group",
			errorMessage: "An error occurred while trying to create the group.", inputLabel: "Name",
			showProgressSpinner: false, rootFeed, parentGroup, null)
	{
		_storage = storage;
	}

	protected override async Task SaveAsync(IFeedView selectedGroup)
	{
		var descriptor = new GroupFeedDescriptor(Input.Trim(), Symbol.Directory);
		await _storage.AddFeedAsync(descriptor, selectedGroup.Identifier);
	}

	protected override void HandleInputChanged()
	{
		IsInputValid = !String.IsNullOrWhiteSpace(Input);
	}

	private readonly IFeedStorage _storage;
}
