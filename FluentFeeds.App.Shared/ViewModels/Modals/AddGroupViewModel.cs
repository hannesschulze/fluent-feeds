using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using FluentFeeds.App.Shared.Models;
using FluentFeeds.App.Shared.Services;
using FluentFeeds.App.Shared.ViewModels.Items.GroupSelection;
using FluentFeeds.Feeds.Base.Nodes;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;

namespace FluentFeeds.App.Shared.ViewModels.Modals;

/// <summary>
/// View model for a modal allowing the user to add a child group to the selected group.
/// </summary>
public sealed class AddGroupViewModel : ObservableObject
{
	public AddGroupViewModel(IFeedService feedService, LoadedFeedProvider provider, IReadOnlyStoredFeedNode parentNode)
	{
		GroupSelectionViewModel = new GroupSelectionViewModel(provider, parentNode, null);

		_feedService = feedService;
		_confirmCommand = new RelayCommand(
			HandleConfirmCommand,
			() => !String.IsNullOrWhiteSpace(Name) && GroupSelectionViewModel.SelectedItem != null);

		GroupSelectionViewModel.PropertyChanged += (s, e) => _confirmCommand.NotifyCanExecuteChanged();
	}

	/// <summary>
	/// Command executed when the user confirms that they want to create the group with the name entered.
	/// </summary>
	public ICommand ConfirmCommand => _confirmCommand;

	/// <summary>
	/// Name entered by the user.
	/// </summary>
	public string Name
	{
		get => _name;
		set
		{
			if (SetProperty(ref _name, value))
			{
				_confirmCommand.NotifyCanExecuteChanged();
			}
		}
	}

	/// <summary>
	/// View model for selecting the parent group.
	/// </summary>
	public GroupSelectionViewModel GroupSelectionViewModel { get; }

	private async void HandleConfirmCommand()
	{
		var group = GroupSelectionViewModel.SelectedItem!.FeedNode;
		await _feedService.AddGroupNodeAsync(group.Identifier, Name.Trim());
	}

	private readonly IFeedService _feedService;
	private readonly RelayCommand _confirmCommand;
	private string _name = String.Empty;
}
