using System;
using System.Threading.Tasks;
using FluentFeeds.App.Shared.Models.Feeds;
using FluentFeeds.App.Shared.Models.Storage;
using FluentFeeds.App.Shared.Resources;

namespace FluentFeeds.App.Shared.ViewModels.Modals;

/// <summary>
/// View model for a modal allowing the user to rename or move a feed.
/// </summary>
public sealed class EditFeedViewModel : FeedDataViewModel
{
	public EditFeedViewModel(IFeedView rootFeed, IFeedView feed, IFeedStorage storage)
		: base(
			title: LocalizedStrings.EditFeedTitle, errorTitle: LocalizedStrings.EditFeedErrorTitle,
			errorMessage: LocalizedStrings.EditFeedErrorMessage, inputLabel: LocalizedStrings.EditFeedInputLabel,
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
