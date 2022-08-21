using FluentFeeds.App.Shared.Models;
using FluentFeeds.App.Shared.Services;
using FluentFeeds.App.Shared.ViewModels.Items.GroupSelection;
using FluentFeeds.Feeds.Base.Nodes;
using System;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using System.Windows.Input;
using FluentFeeds.Feeds.Base.Factories;
using System.Threading.Tasks;
using FluentFeeds.Feeds.Base;

namespace FluentFeeds.App.Shared.ViewModels.Modals;

/// <summary>
/// View model for a modal allowing the user to add a child feed to the selected group.
/// </summary>
public sealed class AddFeedViewModel : ObservableObject
{
	public AddFeedViewModel(
		IFeedService feedService, LoadedFeedProvider provider, IReadOnlyStoredFeedNode parentNode)
	{
		_feedService = feedService;
		_provider = provider;

		GroupSelectionViewModel = new GroupSelectionViewModel(provider, parentNode, null);
		GroupSelectionViewModel.PropertyChanged += (s, e) => CanSave = CheckCanSave();
		_canSave = CheckCanSave();
	}

	/// <summary>
	/// Name entered by the user.
	/// </summary>
	public string Url
	{
		get => _url;
		set
		{
			if (SetProperty(ref _url, value))
			{
				if (!Uri.TryCreate(_url, UriKind.Absolute, out _parsedUrl))
					_parsedUrl = null;
				CanSave = CheckCanSave();
			}
		}
	}

	/// <summary>
	/// Flag indicating if an error message should be shown in the UI.
	/// </summary>
	public bool ShowError
	{
		get => _showError;
		set => SetProperty(ref _showError, value);
	}

	/// <summary>
	/// Flag indicating that the operation is currently in progress.
	/// </summary>
	public bool IsInProgress
	{
		get => _isInProgress;
		private set
		{
			if (SetProperty(ref _isInProgress, value))
			{
				CanSave = CheckCanSave();
			}
		}
	}

	/// <summary>
	/// Flag indicating if the user can create the feed in the view model's current state.
	/// </summary>
	public bool CanSave
	{
		get => _canSave;
		private set => SetProperty(ref _canSave, value);
	}

	/// <summary>
	/// View model for selecting the parent group.
	/// </summary>
	public GroupSelectionViewModel GroupSelectionViewModel { get; }

	/// <summary>
	/// React to the user clicking the "save" button.
	/// </summary>
	/// <returns><c>true</c> if the modal window should close.</returns>
	public async Task<bool> HandleSaveAsync()
	{
		if (!CanSave)
			return false;

		ShowError = false;
		IsInProgress = true;

		Feed feed;
		try
		{
			feed = await _provider.Provider.UrlFeedFactory!.CreateAsync(_provider.FeedStorage, _parsedUrl!);
		}
		catch (Exception)
		{
			ShowError = true;
			IsInProgress = false;
			return false;
		}

		var group = GroupSelectionViewModel.SelectedItem!.FeedNode;
		await _feedService.AddFeedNodeAsync(group.Identifier, feed);
		IsInProgress = false;

		return true;
	}

	private bool CheckCanSave()
	{
		return !IsInProgress && _parsedUrl != null && GroupSelectionViewModel.SelectedItem != null;
	}

	private readonly IFeedService _feedService;
	private readonly LoadedFeedProvider _provider;
	private string _url = String.Empty;
	private bool _showError;
	private bool _isInProgress;
	private bool _canSave;
	private Uri? _parsedUrl;
}
