using System;
using System.Threading.Tasks;
using FluentFeeds.App.Shared.Models.Feeds;
using FluentFeeds.App.Shared.Models.Storage;
using FluentFeeds.App.Shared.Resources;
using FluentFeeds.App.Shared.Services;
using FluentFeeds.Feeds.Base.Factories;
using FluentFeeds.Feeds.Base.Feeds;

namespace FluentFeeds.App.Shared.ViewModels.Modals;

/// <summary>
/// View model for a modal allowing the user to add a child feed to the selected group.
/// </summary>
public sealed class AddFeedViewModel : FeedDataViewModel
{
	public AddFeedViewModel(IUrlFeedFactory factory, IFeedView rootFeed, IFeedView? parentGroup, IFeedStorage storage)
		: base(
			title: LocalizedStrings.AddFeedTitle, errorTitle: LocalizedStrings.AddFeedErrorTitle,
			errorMessage: LocalizedStrings.AddFeedErrorMessage, inputLabel: LocalizedStrings.AddFeedInputLabel,
			showProgressSpinner: true, rootFeed, parentGroup, null)
	{
		_factory = factory;
		_storage = storage;
	}

	protected override async Task SaveAsync(IFeedView selectedGroup)
	{
		var descriptor = new CachedFeedDescriptor(_factory.Create(_parsedUrl!));
		await _storage.AddFeedAsync(descriptor, selectedGroup.Identifier, syncFirst: true);
	}

	protected override void HandleInputChanged()
	{
		if (!Uri.TryCreate(Input, UriKind.Absolute, out _parsedUrl))
			_parsedUrl = null;
		IsInputValid = _parsedUrl != null;
	}

	private readonly IUrlFeedFactory _factory;
	private readonly IFeedStorage _storage;
	private Uri? _parsedUrl;
}
