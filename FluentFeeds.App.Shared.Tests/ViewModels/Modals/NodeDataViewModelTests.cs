using System;
using System.Threading.Tasks;
using FluentFeeds.App.Shared.Tests.Mock;
using FluentFeeds.App.Shared.ViewModels.Modals;
using FluentFeeds.Feeds.Base.Nodes;
using Xunit;

namespace FluentFeeds.App.Shared.Tests.ViewModels.Modals;

public class NodeDataViewModelTests
{
	private sealed class TestViewModel : NodeDataViewModel
	{
		public TestViewModel(
			IReadOnlyStoredFeedNode rootNode, IReadOnlyFeedNode? selectedNode, IReadOnlyFeedNode? forbiddenNode)
			: base("title", "error", "error message", "input", true, rootNode, selectedNode, forbiddenNode)
		{
		}

		public void UpdateInputValid(bool isInputValid) => IsInputValid = isInputValid;

		public void CompleteSave() => _saveCompletionSource?.TrySetResult();

		public void CompleteSave(Exception exception) => _saveCompletionSource?.TrySetException(exception);

		protected override Task SaveAsync(IReadOnlyStoredFeedNode selectedGroup)
		{
			var completionSource = _saveCompletionSource = new TaskCompletionSource();
			return completionSource.Task;
		}

		private TaskCompletionSource? _saveCompletionSource;
	}
	
	public NodeDataViewModelTests()
	{
		ModalService = new ModalServiceMock();
		FeedStorage = new FeedStorageMock(new FeedProviderMock(Guid.Empty));
		RootNode = FeedStorage.AddRootNode(FeedNode.Group("root", null, true));
		GroupNode = FeedStorage.AddNodeAsync(FeedNode.Group("group", null, true), RootNode.Identifier).Result;
		ChildNode = FeedStorage.AddNodeAsync(FeedNode.Group("child", null, false), GroupNode.Identifier).Result;
	}
	
	private ModalServiceMock ModalService { get; }
	private FeedStorageMock FeedStorage { get; }
	private IReadOnlyStoredFeedNode RootNode { get; }
	private IReadOnlyStoredFeedNode GroupNode { get; }
	private IReadOnlyStoredFeedNode ChildNode { get; }

	[Fact]
	public void GroupSelection_Items_NoForbiddenNode()
	{
		var viewModel = new TestViewModel(RootNode, null, null);
		Assert.Collection(
			viewModel.GroupItems,
			item =>
			{
				Assert.Equal("root", item.Title);
				Assert.Equal(0, item.IndentationLevel);
				Assert.True(item.IsSelectable);
			},
			item =>
			{
				Assert.Equal("group", item.Title);
				Assert.Equal(1, item.IndentationLevel);
				Assert.True(item.IsSelectable);
			},
			item =>
			{
				Assert.Equal("child", item.Title);
				Assert.Equal(2, item.IndentationLevel);
				Assert.False(item.IsSelectable);
			});
	}

	[Fact]
	public void GroupSelection_Items_ForbiddenNode()
	{
		var viewModel = new TestViewModel(RootNode, null, GroupNode);
		Assert.Collection(
			viewModel.GroupItems,
			item =>
			{
				Assert.Equal("root", item.Title);
				Assert.Equal(0, item.IndentationLevel);
				Assert.True(item.IsSelectable);
			},
			item =>
			{
				Assert.Equal("group", item.Title);
				Assert.Equal(1, item.IndentationLevel);
				Assert.False(item.IsSelectable);
			},
			item =>
			{
				Assert.Equal("child", item.Title);
				Assert.Equal(2, item.IndentationLevel);
				Assert.False(item.IsSelectable);
			});
	}

	[Fact]
	public void GroupSelection_InitialSelection_ExplicitlySet()
	{
		var viewModel = new TestViewModel(RootNode, GroupNode, null);
		Assert.Equal(viewModel.GroupItems[1], viewModel.SelectedGroup);
	}

	[Fact]
	public void GroupSelection_InitialSelection_NotSet_RootNodeAvailable()
	{
		var viewModel = new TestViewModel(RootNode, null, null);
		Assert.Equal(viewModel.GroupItems[0], viewModel.SelectedGroup);
	}

	[Fact]
	public void GroupSelection_InitialSelection_NotSet_RootNodeForbidden()
	{
		var viewModel = new TestViewModel(RootNode, null, RootNode);
		Assert.Null(viewModel.SelectedGroup);
	}

	[Fact]
	public void GroupSelection_UpdateSelection_ToValidItem()
	{
		var viewModel = new TestViewModel(RootNode, null, null);
		viewModel.SelectedGroup = viewModel.GroupItems[1];
		Assert.Equal(viewModel.GroupItems[1], viewModel.SelectedGroup);
	}

	[Fact]
	public void GroupSelection_UpdateSelection_ToInvalidItem()
	{
		var viewModel = new TestViewModel(RootNode, null, null);
		viewModel.SelectedGroup = viewModel.GroupItems[2];
		Assert.Null(viewModel.SelectedGroup);
	}

	[Fact]
	public void Validation_ValidateInput()
	{
		var viewModel = new TestViewModel(RootNode, null, null);
		Assert.False(viewModel.IsSaveEnabled);
		viewModel.UpdateInputValid(true);
		Assert.True(viewModel.IsSaveEnabled);
		viewModel.UpdateInputValid(false);
		Assert.False(viewModel.IsSaveEnabled);
	}

	[Fact]
	public void Validation_ValidateGroup()
	{
		var viewModel = new TestViewModel(RootNode, null, null);
		viewModel.UpdateInputValid(true);
		viewModel.SelectedGroup = null;
		Assert.False(viewModel.IsSaveEnabled);
		viewModel.SelectedGroup = viewModel.GroupItems[0];
		Assert.True(viewModel.IsSaveEnabled);
	}

	[Fact]
	public void Save_InvalidParameters()
	{
		var viewModel = new TestViewModel(RootNode, null, null);
		Assert.False(viewModel.HandleSaveAsync().Result);
	}

	[Fact]
	public void Save_Success()
	{
		var viewModel = new TestViewModel(RootNode, null, null);
		viewModel.UpdateInputValid(true);
		viewModel.IsErrorVisible = true;
		
		var task = viewModel.HandleSaveAsync();
		Assert.False(viewModel.IsSaveEnabled);
		Assert.False(viewModel.IsErrorVisible);
		Assert.True(viewModel.IsInProgress);
		Assert.False(task.IsCompleted);
		viewModel.CompleteSave();
		Assert.True(viewModel.IsSaveEnabled);
		Assert.False(viewModel.IsErrorVisible);
		Assert.False(viewModel.IsInProgress);
		Assert.True(task.IsCompleted);
		Assert.True(task.Result);
	}

	[Fact]
	public void Save_Error()
	{
		var viewModel = new TestViewModel(RootNode, null, null);
		viewModel.UpdateInputValid(true);
		
		var task = viewModel.HandleSaveAsync();
		viewModel.CompleteSave(new Exception("error"));
		Assert.True(viewModel.IsSaveEnabled);
		Assert.True(viewModel.IsErrorVisible);
		Assert.False(viewModel.IsInProgress);
		Assert.True(task.IsCompleted);
		Assert.False(task.Result);
	}
}
