using System;
using System.Windows.Input;
using FluentFeeds.App.Shared.Models.Feeds;
using FluentFeeds.App.Shared.Models.Storage;
using FluentFeeds.App.Shared.Resources;
using FluentFeeds.App.Shared.Services;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;

namespace FluentFeeds.App.Shared.ViewModels.Modals;

/// <summary>
/// View model for a modal allowing the user to confirm that they want to delete a feed.
/// </summary>
public sealed class DeleteFeedViewModel : ObservableObject
{
	public DeleteFeedViewModel(IModalService modalService, IFeedView feed, IFeedStorage storage)
	{
		_modalService = modalService;
		_feed = feed;
		_storage = storage;
		_confirmCommand = new RelayCommand(HandleConfirmCommand);
		Message = String.Format(LocalizedStrings.DeleteFeedMessage, feed.DisplayName);
	}

	/// <summary>
	/// Command executed when the user confirms that they want to delete the group.
	/// </summary>
	public ICommand ConfirmCommand => _confirmCommand;

	/// <summary>
	/// Text informing the user about the consequences of deleting the feed.
	/// </summary>
	public string Message { get; }

	private async void HandleConfirmCommand()
	{
		try
		{
			await _storage.DeleteFeedAsync(_feed.Identifier);
		}
		catch (Exception)
		{
			_modalService.Show(
				new ErrorViewModel(
					LocalizedStrings.DeleteFeedErrorTitle,
					String.Format(LocalizedStrings.DeleteFeedErrorMessage, Constants.AppName)));
		}
	}

	private readonly IModalService _modalService;
	private readonly IFeedView _feed;
	private readonly IFeedStorage _storage;
	private readonly RelayCommand _confirmCommand;
}
