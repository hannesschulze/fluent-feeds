using System;
using System.Windows.Input;
using FluentFeeds.App.Shared.Models;
using FluentFeeds.App.Shared.Services;
using FluentFeeds.App.Shared.ViewModels.Items.GroupSelection;
using FluentFeeds.Feeds.Base.Nodes;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;

namespace FluentFeeds.App.Shared.ViewModels.Modals;

/// <summary>
/// View model for a modal allowing the user to rename or move a tree node.
/// </summary>
public sealed class EditNodeViewModel : ObservableObject
{
	public EditNodeViewModel(IFeedService feedService, LoadedFeedProvider provider, IReadOnlyStoredFeedNode node)
	{
		var currentParent = feedService.GetParentNode(node.Identifier);
		GroupSelectionViewModel = new GroupSelectionViewModel(provider, currentParent, node);

		_feedService = feedService;
		_node = node;
		_name = node.ActualTitle ?? String.Empty;
		_saveCommand = new RelayCommand(
			HandleSaveCommand,
			() => !String.IsNullOrWhiteSpace(Name) && GroupSelectionViewModel.SelectedItem != null);

		GroupSelectionViewModel.PropertyChanged += (s, e) => _saveCommand.NotifyCanExecuteChanged();
	}

	/// <summary>
	/// Command executed when the user confirms that they want to save the changes.
	/// </summary>
	public ICommand SaveCommand => _saveCommand;

	/// <summary>
	/// The updated name.
	/// </summary>
	public string Name
	{
		get => _name;
		set
		{
			if (SetProperty(ref _name, value))
			{
				_saveCommand.NotifyCanExecuteChanged();
			}
		}
	}

	/// <summary>
	/// View model for selecting the new parent group.
	/// </summary>
	public GroupSelectionViewModel GroupSelectionViewModel { get; }

	private async void HandleSaveCommand()
	{
		var newParent = GroupSelectionViewModel.SelectedItem!.FeedNode;
		await _feedService.EditNodeAsync(_node.Identifier, newParent.Identifier, Name.Trim());
	}

	private readonly IFeedService _feedService;
	private readonly IReadOnlyStoredFeedNode _node;
	private readonly RelayCommand _saveCommand;
	private string _name;
}
