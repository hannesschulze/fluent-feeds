using System;
using FluentFeeds.App.Shared.Services;
using FluentFeeds.App.Shared.ViewModels.Items.Navigation;
using FluentFeeds.App.Shared.ViewModels.Modals;

namespace FluentFeeds.App.Shared.Tests.Mock;

public sealed class ModalServiceMock : IModalService
{
	public sealed class ShowFeedDataModalEventArgs : System.EventArgs
	{
		public ShowFeedDataModalEventArgs(FeedDataViewModel viewModel, NavigationItemViewModel relatedItem)
		{
			ViewModel = viewModel;
			RelatedItem = relatedItem;
		}
		
		public FeedDataViewModel ViewModel { get; }
		public NavigationItemViewModel RelatedItem { get; }
	}
	
	public sealed class ShowDeleteFeedModalEventArgs : System.EventArgs
	{
		public ShowDeleteFeedModalEventArgs(DeleteFeedViewModel viewModel, NavigationItemViewModel relatedItem)
		{
			ViewModel = viewModel;
			RelatedItem = relatedItem;
		}
		
		public DeleteFeedViewModel ViewModel { get; }
		public NavigationItemViewModel RelatedItem { get; }
	}
	
	public sealed class ShowErrorModalEventArgs : System.EventArgs
	{
		public ShowErrorModalEventArgs(ErrorViewModel viewModel)
		{
			ViewModel = viewModel;
		}
		
		public ErrorViewModel ViewModel { get; }
	}

	public event EventHandler<ShowFeedDataModalEventArgs>? ShowFeedDataModal;
	
	public event EventHandler<ShowDeleteFeedModalEventArgs>? ShowDeleteFeedModal;
	
	public event EventHandler<ShowErrorModalEventArgs>? ShowErrorModal;

	public void Show(FeedDataViewModel viewModel, NavigationItemViewModel relatedItem) =>
		ShowFeedDataModal?.Invoke(this, new ShowFeedDataModalEventArgs(viewModel, relatedItem));

	public void Show(DeleteFeedViewModel viewModel, NavigationItemViewModel relatedItem) =>
		ShowDeleteFeedModal?.Invoke(this, new ShowDeleteFeedModalEventArgs(viewModel, relatedItem));

	public void Show(ErrorViewModel viewModel) =>
		ShowErrorModal?.Invoke(this, new ShowErrorModalEventArgs(viewModel));
}
