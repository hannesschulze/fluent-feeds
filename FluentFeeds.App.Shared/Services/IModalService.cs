using FluentFeeds.App.Shared.ViewModels.Items.Navigation;
using FluentFeeds.App.Shared.ViewModels.Modals;

namespace FluentFeeds.App.Shared.Services;

/// <summary>
/// Service for showing modal dialogs/flyouts.
/// </summary>
public interface IModalService
{
	void ShowModal(AddFeedViewModel viewModel, NavigationItemViewModel relatedItem);
	void ShowModal(AddGroupViewModel viewModel, NavigationItemViewModel relatedItem);
	void ShowModal(DeleteNodeViewModel viewModel, NavigationItemViewModel relatedItem);
	void ShowModal(EditNodeViewModel viewModel, NavigationItemViewModel relatedItem);
}
