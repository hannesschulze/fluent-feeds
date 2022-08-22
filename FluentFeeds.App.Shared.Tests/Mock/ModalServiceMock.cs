using System;
using FluentFeeds.App.Shared.Services;
using FluentFeeds.App.Shared.ViewModels.Items.Navigation;
using FluentFeeds.App.Shared.ViewModels.Modals;

namespace FluentFeeds.App.Shared.Tests.Mock;

public sealed class ModalServiceMock : IModalService
{
	public sealed class ShowNodeDataModalEventArgs : EventArgs
	{
		public ShowNodeDataModalEventArgs(NodeDataViewModel viewModel, NavigationItemViewModel relatedItem)
		{
			ViewModel = viewModel;
			RelatedItem = relatedItem;
		}
		
		public NodeDataViewModel ViewModel { get; }
		public NavigationItemViewModel RelatedItem { get; }
	}
	
	public sealed class ShowDeleteNodeModalEventArgs : EventArgs
	{
		public ShowDeleteNodeModalEventArgs(DeleteNodeViewModel viewModel, NavigationItemViewModel relatedItem)
		{
			ViewModel = viewModel;
			RelatedItem = relatedItem;
		}
		
		public DeleteNodeViewModel ViewModel { get; }
		public NavigationItemViewModel RelatedItem { get; }
	}
	
	public sealed class ShowErrorModalEventArgs : EventArgs
	{
		public ShowErrorModalEventArgs(ErrorViewModel viewModel)
		{
			ViewModel = viewModel;
		}
		
		public ErrorViewModel ViewModel { get; }
	}

	public event EventHandler<ShowNodeDataModalEventArgs>? ShowNodeDataModal;
	
	public event EventHandler<ShowDeleteNodeModalEventArgs>? ShowDeleteNodeModal;
	
	public event EventHandler<ShowErrorModalEventArgs>? ShowErrorModal;

	public void Show(NodeDataViewModel viewModel, NavigationItemViewModel relatedItem) =>
		ShowNodeDataModal?.Invoke(this, new ShowNodeDataModalEventArgs(viewModel, relatedItem));

	public void Show(DeleteNodeViewModel viewModel, NavigationItemViewModel relatedItem) =>
		ShowDeleteNodeModal?.Invoke(this, new ShowDeleteNodeModalEventArgs(viewModel, relatedItem));

	public void Show(ErrorViewModel viewModel) =>
		ShowErrorModal?.Invoke(this, new ShowErrorModalEventArgs(viewModel));
}
