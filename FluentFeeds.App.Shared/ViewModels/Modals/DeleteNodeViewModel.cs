using System.Windows.Input;
using FluentFeeds.App.Shared.Services;
using FluentFeeds.Feeds.Base.Nodes;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;

namespace FluentFeeds.App.Shared.ViewModels.Modals;

/// <summary>
/// View model for a modal allowing the user to confirm that they want to delete a node.
/// </summary>
public sealed class DeleteNodeViewModel : ObservableObject
{
	public DeleteNodeViewModel(IFeedService feedService, IReadOnlyStoredFeedNode node)
	{
		_feedService = feedService;
		_node = node;
		_confirmCommand = new RelayCommand(HandleConfirmCommand);
		InfoText = $"This action will permanently delete \"{node.ActualTitle}\" and all of its children. Are you sure you want to continue?";
	}

	/// <summary>
	/// Command executed when the user confirms that they want to delete the group.
	/// </summary>
	public ICommand ConfirmCommand => _confirmCommand;

	/// <summary>
	/// Text informing the user about the consequences of deleting the node.
	/// </summary>
	public string InfoText { get; }

	private async void HandleConfirmCommand()
	{
		await _feedService.DeleteNodeAsync(_node.Identifier);
	}

	private readonly IFeedService _feedService;
	private readonly IReadOnlyStoredFeedNode _node;
	private readonly RelayCommand _confirmCommand;
}
