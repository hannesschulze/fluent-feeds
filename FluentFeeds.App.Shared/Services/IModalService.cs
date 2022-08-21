using FluentFeeds.App.Shared.ViewModels.Items.Navigation;
using FluentFeeds.App.Shared.ViewModels.Modals;

namespace FluentFeeds.App.Shared.Services;

/// <summary>
/// Service for showing modal dialogs/flyouts.
/// </summary>
public interface IModalService
{
	void ShowModal(NodeDataViewModel viewModel, NavigationItemViewModel relatedItem);
	void ShowModal(DeleteNodeViewModel viewModel, NavigationItemViewModel relatedItem);
}
