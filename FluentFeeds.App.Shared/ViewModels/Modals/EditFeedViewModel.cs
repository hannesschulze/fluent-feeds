using System;
using System.Threading.Tasks;
using FluentFeeds.App.Shared.Models.Feeds;
using FluentFeeds.App.Shared.Models.Storage;

namespace FluentFeeds.App.Shared.ViewModels.Modals;

/// <summary>
/// View model for a modal allowing the user to rename or move a feed.
/// </summary>
public sealed class EditFeedViewModel : FeedDataViewModel
{
	public EditFeedViewModel(IFeedView rootFeed, IFeedView feed, IFeedStorage storage)
		: base(
			title: "Edit an item", errorTitle: "Unable to edit the item",
			errorMessage: "An error occurred while trying to apply changes to the item.", inputLabel: "Name",
			showProgressSpinner: false, rootFeed, feed.Parent, feed)
	{
		_feed = feed;
		_storage = storage;
		Input = feed.DisplayName;
	}

	protected override async Task SaveAsync(IFeedView selectedGroup)
	{
		var newTitle = Input.Trim();
		if (newTitle != _feed.DisplayName)
		{
			await _storage.RenameFeedAsync(_feed.Identifier, newTitle);
		}

		await _storage.MoveFeedAsync(_feed.Identifier, selectedGroup.Identifier);
	}

	protected override void HandleInputChanged()
	{
		IsInputValid = !String.IsNullOrWhiteSpace(Input);
	}

	private readonly IFeedView _feed;
	private readonly IFeedStorage _storage;
}
