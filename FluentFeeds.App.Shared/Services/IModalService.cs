using FluentFeeds.App.Shared.ViewModels.Items.Navigation;
using FluentFeeds.App.Shared.ViewModels.Modals;

namespace FluentFeeds.App.Shared.Services;

/// <summary>
/// Service for showing modal dialogs/flyouts.
/// </summary>
public interface IModalService
{
	void Show(NodeDataViewModel viewModel, NavigationItemViewModel relatedItem);
	void Show(DeleteNodeViewModel viewModel, NavigationItemViewModel relatedItem);
	void Show(ErrorViewModel viewModel);
}
