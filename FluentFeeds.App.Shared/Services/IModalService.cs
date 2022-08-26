using FluentFeeds.App.Shared.ViewModels.ListItems.Navigation;
using FluentFeeds.App.Shared.ViewModels.Modals;

namespace FluentFeeds.App.Shared.Services;

/// <summary>
/// Service for showing modal dialogs/flyouts.
/// </summary>
public interface IModalService
{
	void Show(FeedDataViewModel viewModel, NavigationItemViewModel relatedItem);
	void Show(DeleteFeedViewModel viewModel, NavigationItemViewModel relatedItem);
	void Show(ErrorViewModel viewModel);
}
