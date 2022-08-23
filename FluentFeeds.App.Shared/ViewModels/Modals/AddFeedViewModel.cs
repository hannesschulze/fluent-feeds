using System;
using System.Threading.Tasks;
using FluentFeeds.Feeds.Base;
using FluentFeeds.Feeds.Base.Factories;
using FluentFeeds.Feeds.Base.Nodes;

namespace FluentFeeds.App.Shared.ViewModels.Modals;

/// <summary>
/// View model for a modal allowing the user to add a child feed to the selected group.
/// </summary>
public sealed class AddFeedViewModel : NodeDataViewModel
{
	public AddFeedViewModel(IUrlFeedFactory factory, IReadOnlyStoredFeedNode rootNode, IReadOnlyFeedNode? parentGroup)
		: base(
			title: "Add a feed", errorTitle: "Unable to create the feed",
			errorMessage: "An error occurred while trying to create a feed for the provided URL.", inputLabel: "URL",
			showProgressSpinner: true, rootNode, parentGroup, null)
	{
		_factory = factory;
	}

	protected override async Task SaveAsync(IReadOnlyStoredFeedNode selectedGroup)
	{
		var storage = selectedGroup.Storage;
		var feed = await _factory.CreateAsync(storage, _parsedUrl!);
		var node = FeedNode.Custom(feed, null, null, isUserCustomizable: true);
		await storage.AddNodeAsync(node, selectedGroup.Identifier);

		SynchronizeNewFeedAsync(feed);
	}

	private static async void SynchronizeNewFeedAsync(Feed newFeed)
	{
		try
		{
			await newFeed.SynchronizeAsync();
		}
		catch (Exception)
		{
			// TODO: Show error
		}
	}

	protected override void HandleInputChanged()
	{
		if (!Uri.TryCreate(Input, UriKind.Absolute, out _parsedUrl))
			_parsedUrl = null;
		IsInputValid = _parsedUrl != null;
	}

	private readonly IUrlFeedFactory _factory;
	private Uri? _parsedUrl;
}
