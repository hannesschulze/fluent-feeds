using FluentFeeds.App.Shared.ViewModels.Items.GroupSelection;
using System;
using System.Collections.ObjectModel;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using System.Threading.Tasks;
using FluentFeeds.App.Shared.Models.Feeds;

namespace FluentFeeds.App.Shared.ViewModels.Modals;

/// <summary>
/// Base view model for updating feed data. The view model contains a text input and a group selection menu.
/// </summary>
public abstract class FeedDataViewModel : ObservableObject
{
	/// <summary>
	/// Create a feed-data based view model.
	/// </summary>
	/// <param name="title">Title for the dialog.</param>
	/// <param name="errorTitle">Error title shown if an exception is thrown.</param>
	/// <param name="errorMessage">Error message shown if an exception is thrown.</param>
	/// <param name="inputLabel">Label for the <c>Input</c> field.</param>
	/// <param name="showProgressSpinner">
	/// Flag indicating if a progress spinner should be shown during the save action.
	/// </param>
	/// <param name="rootFeed">The root feed displayed in the group selection.</param>
	/// <param name="selectedGroup">Initially selected group (<c>rootFeed</c> is used as a fallback).</param>
	/// <param name="forbiddenGroup">A group in the menu which cannot be selected (including its children).</param>
	protected FeedDataViewModel(
		string title, string errorTitle, string errorMessage, string inputLabel, bool showProgressSpinner,
		IFeedView rootFeed, IFeedView? selectedGroup, IFeedView? forbiddenGroup)
	{
		_isSaveEnabled = CheckCanSave();
		_showProgressSpinner = showProgressSpinner;
		_showProgressSpinner = showProgressSpinner;
		if (selectedGroup == null && rootFeed.IsUserCustomizable && rootFeed.Children != null &&
			forbiddenGroup != rootFeed)
		{
			selectedGroup = rootFeed;
		}
		AddGroupItem(rootFeed, selectedGroup, forbiddenGroup);

		Title = title;
		ErrorTitle = errorTitle;
		ErrorMessage = errorMessage;
		InputLabel = inputLabel;
		GroupItems = new ReadOnlyObservableCollection<GroupSelectionItemViewModel>(_groupItems);
	}
	
	private void AddGroupItem(
		IFeedView feed, IFeedView? selectedGroup, IFeedView? forbiddenGroup, bool isInForbiddenGroup = false,
		int indentationLevel = 0)
	{
		isInForbiddenGroup = isInForbiddenGroup || feed == forbiddenGroup;
		var item = new GroupSelectionItemViewModel(
			feed, indentationLevel,
			isSelectable: !isInForbiddenGroup && feed.IsUserCustomizable && feed.Children != null);
		if (feed == selectedGroup)
			_selectedGroup = item;
		_groupItems.Add(item);

		// Add children right after this item.
		if (feed.Children != null)
		{
			foreach (var child in feed.Children)
			{
				AddGroupItem(child, selectedGroup, forbiddenGroup, isInForbiddenGroup, indentationLevel + 1);
			}
		}
	}

	private bool CheckCanSave()
	{
		return !IsInProgress && IsInputValid && SelectedGroup != null;
	}

	protected virtual void HandleInputChanged()
	{
	}

	protected abstract Task SaveAsync(IFeedView selectedGroup);

	protected bool IsInputValid
	{
		get => _isInputValid;
		set
		{
			_isInputValid = value;
			IsSaveEnabled = CheckCanSave();
		}
	}

	/// <summary>
	/// Title for the dialog.
	/// </summary>
	public string Title { get; }

	/// <summary>
	/// Title shown if <see cref="IsErrorVisible"/> is set to true.
	/// </summary>
	public string ErrorTitle { get; }

	/// <summary>
	/// Message shown if <see cref="IsErrorVisible"/> is set to true.
	/// </summary>
	public string ErrorMessage { get; }

	/// <summary>
	/// Label for the <see cref="Input"/> field.
	/// </summary>
	public string InputLabel { get; }

	/// <summary>
	/// Current text input value.
	/// </summary>
	public string Input
	{
		get => _input;
		set
		{
			if (SetProperty(ref _input, value))
			{
				HandleInputChanged();
			}
		}
	}

	/// <summary>
	/// Flag indicating if an error message should be shown in the UI.
	/// </summary>
	public bool IsErrorVisible
	{
		get => _isErrorVisible;
		set => SetProperty(ref _isErrorVisible, value);
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
				IsSaveEnabled = CheckCanSave();
			}
		}
	}

	/// <summary>
	/// Flag indicating if the user can create the feed in the view model's current state.
	/// </summary>
	public bool IsSaveEnabled
	{
		get => _isSaveEnabled;
		private set => SetProperty(ref _isSaveEnabled, value);
	}

	/// <summary>
	/// Available items which can be used to select the location.
	/// </summary>
	public ReadOnlyObservableCollection<GroupSelectionItemViewModel> GroupItems { get; }

	/// <summary>
	/// The currently selected group.
	/// </summary>
	public GroupSelectionItemViewModel? SelectedGroup
	{
		get => _selectedGroup;
		set
		{
			if (SetProperty(ref _selectedGroup, value is { IsSelectable: true } ? value : null))
			{
				IsSaveEnabled = CheckCanSave();
			}
		}
	}

	/// <summary>
	/// Respond to the user clicking the "save" button.
	/// </summary>
	/// <returns><c>true</c> if the modal window should close.</returns>
	public async Task<bool> HandleSaveAsync()
	{
		if (!IsSaveEnabled)
			return false;

		IsErrorVisible = false;
		if (_showProgressSpinner)
		{
			IsInProgress = true;
		}

		try
		{
			await SaveAsync(SelectedGroup!.Feed);
		}
		catch (Exception)
		{
			IsErrorVisible = true;
			IsInProgress = false;
			return false;
		}

		IsInProgress = false;
		return true;
	}

	private readonly ObservableCollection<GroupSelectionItemViewModel> _groupItems = new();
	private readonly bool _showProgressSpinner;
	private GroupSelectionItemViewModel? _selectedGroup;
	private string _input = String.Empty;
	private bool _isInputValid;
	private bool _isErrorVisible;
	private bool _isInProgress;
	private bool _isSaveEnabled;
}
