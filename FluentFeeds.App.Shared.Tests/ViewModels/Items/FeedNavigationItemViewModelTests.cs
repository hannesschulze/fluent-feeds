using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentFeeds.App.Shared.Models;
using FluentFeeds.App.Shared.Tests.Mock;
using FluentFeeds.App.Shared.ViewModels.Items.Navigation;
using FluentFeeds.Common;
using FluentFeeds.Feeds.Base;
using FluentFeeds.Feeds.Base.Nodes;
using Xunit;

namespace FluentFeeds.App.Shared.Tests.ViewModels.Items;

public class FeedNavigationItemViewModelTests
{
	private ModalServiceMock ModalService { get; } = new();
	
	[Fact]
	public void Metadata_GroupNode()
	{
		var node = FeedNode.Group("Title", Symbol.Directory, isUserCustomizable: true);
		var item = new FeedNavigationItemViewModel(
			ModalService, node, null, new Dictionary<IReadOnlyFeedNode, NavigationItemViewModel>());
		Assert.Equal("Title",item.Title);
		Assert.Equal(Symbol.Directory, item.Symbol);
		Assert.Equal(NavigationRoute.Feed(node), item.Destination);
		Assert.True(item.IsExpandable);
		Assert.Empty(item.Children);

		var childNode = FeedNode.Group(null, null, false);
		node.Children!.Add(childNode);
		var childItem = Assert.IsType<FeedNavigationItemViewModel>(Assert.Single(item.Children));
		Assert.Equal(childNode, childItem.FeedNode);
	}
	
	[Fact]
	public void Metadata_LeafNode()
	{
		var feed = new FeedMock(Guid.Empty);
		feed.UpdateMetadata(new FeedMetadata { Name = "Feed", Symbol = Symbol.Web });
		var node = FeedNode.Custom(feed, null, null, isUserCustomizable: true);
		var item = new FeedNavigationItemViewModel(
			ModalService, node, null, new Dictionary<IReadOnlyFeedNode, NavigationItemViewModel>());
		Assert.Equal("Feed",item.Title);
		Assert.Equal(Symbol.Web, item.Symbol);
		Assert.Equal(NavigationRoute.Feed(node), item.Destination);
		Assert.False(item.IsExpandable);
		Assert.Empty(item.Children);
	}

	[Fact]
	public void AvailableActions_NodeIsNotStored()
	{
		var node = FeedNode.Group(null, null, true);
		var item = new FeedNavigationItemViewModel(
			ModalService, node, null, new Dictionary<IReadOnlyFeedNode, NavigationItemViewModel>());
		Assert.Empty(item.Actions);
	}

	[Fact]
	public void AvailableActions_RootNode_UserCustomizable_WithUrlFactory()
	{
		var feedStorage = new FeedStorageMock(new FeedProviderMock(Guid.Empty));
		var node = feedStorage.AddRootNode(FeedNode.Group(null, null, isUserCustomizable: true));
		var item = new FeedNavigationItemViewModel(
			ModalService, node, node, new Dictionary<IReadOnlyFeedNode, NavigationItemViewModel>());
		Assert.Collection(
			item.Actions,
			action => Assert.Equal("Add feed…", action.Title),
			action => Assert.Equal("Add group…", action.Title));
	}

	[Fact]
	public void AvailableActions_RootNode_UserCustomizable_WithoutUrlFactory()
	{
		var feedStorage = new FeedStorageMock(new FeedProviderMock(Guid.Empty, hasUrlFactory: false));
		var node = feedStorage.AddRootNode(FeedNode.Group(null, null, isUserCustomizable: true));
		var item = new FeedNavigationItemViewModel(
			ModalService, node, node, new Dictionary<IReadOnlyFeedNode, NavigationItemViewModel>());
		Assert.Collection(
			item.Actions,
			action => Assert.Equal("Add group…", action.Title));
	}

	[Fact]
	public void AvailableActions_RootNode_NotUserCustomizable()
	{
		var feedStorage = new FeedStorageMock(new FeedProviderMock(Guid.Empty));
		var node = feedStorage.AddRootNode(FeedNode.Group(null, null, isUserCustomizable: false));
		var item = new FeedNavigationItemViewModel(
			ModalService, node, node, new Dictionary<IReadOnlyFeedNode, NavigationItemViewModel>());
		Assert.Empty(item.Actions);
	}

	[Fact]
	public void AvailableActions_GroupNode_UserCustomizable_WithUrlFactory()
	{
		var feedStorage = new FeedStorageMock(new FeedProviderMock(Guid.Empty));
		var root = feedStorage.AddRootNode(FeedNode.Group(null, null, true));
		var node = feedStorage.AddNodeAsync(
			FeedNode.Group(null, null, isUserCustomizable: true), root.Identifier).Result;
		var item = new FeedNavigationItemViewModel(
			ModalService, node, root, new Dictionary<IReadOnlyFeedNode, NavigationItemViewModel>());
		Assert.Collection(
			item.Actions,
			action => Assert.Equal("Add feed…", action.Title),
			action => Assert.Equal("Add group…", action.Title),
			action => Assert.Equal("Edit…", action.Title),
			action => Assert.Equal("Delete", action.Title));
	}

	[Fact]
	public void AvailableActions_GroupNode_UserCustomizable_WithoutUrlFactory()
	{
		var feedStorage = new FeedStorageMock(new FeedProviderMock(Guid.Empty, hasUrlFactory: false));
		var root = feedStorage.AddRootNode(FeedNode.Group(null, null, true));
		var node = feedStorage.AddNodeAsync(
			FeedNode.Group(null, null, isUserCustomizable: true), root.Identifier).Result;
		var item = new FeedNavigationItemViewModel(
			ModalService, node, root, new Dictionary<IReadOnlyFeedNode, NavigationItemViewModel>());
		Assert.Collection(
			item.Actions,
			action => Assert.Equal("Add group…", action.Title),
			action => Assert.Equal("Edit…", action.Title),
			action => Assert.Equal("Delete", action.Title));
	}

	[Fact]
	public void AvailableActions_GroupNode_NotUserCustomizable()
	{
		var feedStorage = new FeedStorageMock(new FeedProviderMock(Guid.Empty));
		var root = feedStorage.AddRootNode(FeedNode.Group(null, null, true));
		var node = feedStorage.AddNodeAsync(
			FeedNode.Group(null, null, isUserCustomizable: false), root.Identifier).Result;
		var item = new FeedNavigationItemViewModel(
			ModalService, node, root, new Dictionary<IReadOnlyFeedNode, NavigationItemViewModel>());
		Assert.Empty(item.Actions);
	}

	[Fact]
	public void AvailableActions_LeafNode_UserCustomizable()
	{
		var feedStorage = new FeedStorageMock(new FeedProviderMock(Guid.Empty));
		var root = feedStorage.AddRootNode(FeedNode.Group(null, null, true));
		var node = feedStorage.AddNodeAsync(
			FeedNode.Custom(new FeedMock(Guid.Empty), null, null, isUserCustomizable: true), root.Identifier).Result;
		var item = new FeedNavigationItemViewModel(
			ModalService, node, root, new Dictionary<IReadOnlyFeedNode, NavigationItemViewModel>());
		Assert.Collection(
			item.Actions,
			action => Assert.Equal("Edit…", action.Title),
			action => Assert.Equal("Delete", action.Title));
	}

	[Fact]
	public void AvailableActions_LeafNode_NotUserCustomizable()
	{
		var feedStorage = new FeedStorageMock(new FeedProviderMock(Guid.Empty));
		var root = feedStorage.AddRootNode(FeedNode.Group(null, null, true));
		var node = feedStorage.AddNodeAsync(
			FeedNode.Custom(new FeedMock(Guid.Empty), null, null, isUserCustomizable: false), root.Identifier).Result;
		var item = new FeedNavigationItemViewModel(
			ModalService, node, root, new Dictionary<IReadOnlyFeedNode, NavigationItemViewModel>());
		Assert.Empty(item.Actions);
	}

	[Fact]
	public void UpdateProperties()
	{
		var feedStorage = new FeedStorageMock(new FeedProviderMock(Guid.Empty));
		var root = feedStorage.AddRootNode(FeedNode.Group(null, null, true));
		var feed = new FeedMock(Guid.Empty);
		feed.UpdateMetadata(new FeedMetadata { Name = "Feed", Symbol = Symbol.Web });
		var node = feedStorage.AddNodeAsync(
			FeedNode.Custom(feed, null, null, isUserCustomizable: true), root.Identifier).Result;
		var item = new FeedNavigationItemViewModel(
			ModalService, node, root, new Dictionary<IReadOnlyFeedNode, NavigationItemViewModel>());
		feed.UpdateMetadata(new FeedMetadata { Name = "Updated", Symbol = Symbol.Feed });
		((StoredFeedNode)node).IsUserCustomizable = false;
		Assert.Equal("Updated", item.Title);
		Assert.Equal(Symbol.Feed, item.Symbol);
		Assert.Empty(item.Actions);
	}

	[Fact]
	public async Task Actions_AddFeed()
	{
		var feedStorage = new FeedStorageMock(new FeedProviderMock(Guid.Empty, hasUrlFactory: true));
		var root = feedStorage.AddRootNode(FeedNode.Group("root", null, isUserCustomizable: true));
		var group = await feedStorage.AddNodeAsync(
			FeedNode.Group("group", null, isUserCustomizable: true), root.Identifier);
		var item = new FeedNavigationItemViewModel(
			ModalService, group, root, new Dictionary<IReadOnlyFeedNode, NavigationItemViewModel>());
		var modalArgs = Assert.Raises<ModalServiceMock.ShowNodeDataModalEventArgs>(
			h => ModalService.ShowNodeDataModal += h, h => ModalService.ShowNodeDataModal -= h,
			() => item.Actions[0].Command.Execute(null)).Arguments;
		Assert.Equal(item, modalArgs.RelatedItem);
		
		// Location
		Assert.Collection(
			modalArgs.ViewModel.GroupItems,
			groupItem =>
			{
				Assert.Equal("root", groupItem.Title);
				Assert.True(groupItem.IsSelectable);
			},
			groupItem =>
			{
				Assert.Equal("group", groupItem.Title);
				Assert.True(groupItem.IsSelectable);
			});
		Assert.Equal(modalArgs.ViewModel.GroupItems[1], modalArgs.ViewModel.SelectedGroup);
		
		// Input
		modalArgs.ViewModel.Input = "test";
		Assert.False(modalArgs.ViewModel.IsSaveEnabled);
		modalArgs.ViewModel.Input = "https://www.example.com";
		Assert.True(modalArgs.ViewModel.IsSaveEnabled);
		
		// Save
		Assert.True(await modalArgs.ViewModel.HandleSaveAsync());
		var node = Assert.Single(group.Children!);
		var feed = Assert.IsType<FeedMock>(node.Feed);
		Assert.Equal(new Uri("https://www.example.com"), feed.Url);
	}

	[Fact]
	public async Task Actions_AddGroup()
	{
		var feedStorage = new FeedStorageMock(new FeedProviderMock(Guid.Empty, hasUrlFactory: false));
		var root = feedStorage.AddRootNode(FeedNode.Group("root", null, isUserCustomizable: true));
		var group = await feedStorage.AddNodeAsync(
			FeedNode.Group("group", null, isUserCustomizable: true), root.Identifier);
		var item = new FeedNavigationItemViewModel(
			ModalService, group, root, new Dictionary<IReadOnlyFeedNode, NavigationItemViewModel>());
		var modalArgs = Assert.Raises<ModalServiceMock.ShowNodeDataModalEventArgs>(
			h => ModalService.ShowNodeDataModal += h, h => ModalService.ShowNodeDataModal -= h,
			() => item.Actions[0].Command.Execute(null)).Arguments;
		Assert.Equal(item, modalArgs.RelatedItem);
		
		// Location
		Assert.Collection(
			modalArgs.ViewModel.GroupItems,
			groupItem =>
			{
				Assert.Equal("root", groupItem.Title);
				Assert.True(groupItem.IsSelectable);
			},
			groupItem =>
			{
				Assert.Equal("group", groupItem.Title);
				Assert.True(groupItem.IsSelectable);
			});
		Assert.Equal(modalArgs.ViewModel.GroupItems[1], modalArgs.ViewModel.SelectedGroup);
		
		// Input
		modalArgs.ViewModel.Input = "  ";
		Assert.False(modalArgs.ViewModel.IsSaveEnabled);
		modalArgs.ViewModel.Input = " test ";
		Assert.True(modalArgs.ViewModel.IsSaveEnabled);
		
		// Save
		Assert.True(await modalArgs.ViewModel.HandleSaveAsync());
		var node = Assert.Single(group.Children!);
		Assert.Equal(FeedNodeType.Group, node.Type);
		Assert.Equal("test", node.Title);
	}

	[Fact]
	public async Task Actions_EditNode()
	{
		var feedStorage = new FeedStorageMock(new FeedProviderMock(Guid.Empty, hasUrlFactory: false));
		var root = feedStorage.AddRootNode(FeedNode.Group("root", null, isUserCustomizable: true));
		var group = await feedStorage.AddNodeAsync(
			FeedNode.Group("group", null, isUserCustomizable: true), root.Identifier);
		var node = await feedStorage.AddNodeAsync(
			FeedNode.Group("node", null, isUserCustomizable: true), group.Identifier);
		var item = new FeedNavigationItemViewModel(
			ModalService, node, root, new Dictionary<IReadOnlyFeedNode, NavigationItemViewModel>());
		var modalArgs = Assert.Raises<ModalServiceMock.ShowNodeDataModalEventArgs>(
			h => ModalService.ShowNodeDataModal += h, h => ModalService.ShowNodeDataModal -= h,
			() => item.Actions[1].Command.Execute(null)).Arguments;
		Assert.Equal(item, modalArgs.RelatedItem);
		
		// Location
		Assert.Collection(
			modalArgs.ViewModel.GroupItems,
			groupItem =>
			{
				Assert.Equal("root", groupItem.Title);
				Assert.True(groupItem.IsSelectable);
			},
			groupItem =>
			{
				Assert.Equal("group", groupItem.Title);
				Assert.True(groupItem.IsSelectable);
			},
			groupItem =>
			{
				Assert.Equal("node", groupItem.Title);
				Assert.False(groupItem.IsSelectable);
			});
		Assert.Equal(modalArgs.ViewModel.GroupItems[1], modalArgs.ViewModel.SelectedGroup);
		modalArgs.ViewModel.SelectedGroup = modalArgs.ViewModel.GroupItems[0];
		
		// Input
		modalArgs.ViewModel.Input = "  ";
		Assert.False(modalArgs.ViewModel.IsSaveEnabled);
		modalArgs.ViewModel.Input = " test ";
		Assert.True(modalArgs.ViewModel.IsSaveEnabled);
		
		// Save
		Assert.True(await modalArgs.ViewModel.HandleSaveAsync());
		Assert.Collection(
			root.Children!,
			child => Assert.Equal(group, child),
			child => Assert.Equal(node, child));
		Assert.Equal("test", node.Title);
	}

	[Fact]
	public async Task Actions_DeleteNode_Success()
	{
		var feedStorage = new FeedStorageMock(new FeedProviderMock(Guid.Empty, hasUrlFactory: false));
		var root = feedStorage.AddRootNode(FeedNode.Group("root", null, isUserCustomizable: true));
		var node = await feedStorage.AddNodeAsync(
			FeedNode.Group("node", null, isUserCustomizable: true), root.Identifier);
		var item = new FeedNavigationItemViewModel(
			ModalService, node, root, new Dictionary<IReadOnlyFeedNode, NavigationItemViewModel>());
		var modalArgs = Assert.Raises<ModalServiceMock.ShowDeleteNodeModalEventArgs>(
			h => ModalService.ShowDeleteNodeModal += h, h => ModalService.ShowDeleteNodeModal -= h,
			() => item.Actions[2].Command.Execute(null)).Arguments;
		Assert.Equal(item, modalArgs.RelatedItem);
		
		modalArgs.ViewModel.ConfirmCommand.Execute(null);
		Assert.Empty(root.Children!);
	}

	[Fact]
	public async Task Actions_DeleteNode_Error()
	{
		var feedStorage = new FeedStorageMock(new FeedProviderMock(Guid.Empty, hasUrlFactory: false));
		var root = feedStorage.AddRootNode(FeedNode.Group("root", null, isUserCustomizable: true));
		var node = await feedStorage.AddNodeAsync(
			FeedNode.Group("node", null, isUserCustomizable: true), root.Identifier);
		var item = new FeedNavigationItemViewModel(
			ModalService, node, root, new Dictionary<IReadOnlyFeedNode, NavigationItemViewModel>());
		var modalArgs = Assert.Raises<ModalServiceMock.ShowDeleteNodeModalEventArgs>(
			h => ModalService.ShowDeleteNodeModal += h, h => ModalService.ShowDeleteNodeModal -= h,
			() => item.Actions[2].Command.Execute(null)).Arguments;
		Assert.Equal(item, modalArgs.RelatedItem);

		feedStorage.DeleteNodeFails = true;
		var errorArgs = Assert.Raises<ModalServiceMock.ShowErrorModalEventArgs>(
			h => ModalService.ShowErrorModal += h, h => ModalService.ShowErrorModal -= h,
			() => modalArgs.ViewModel.ConfirmCommand.Execute(null)).Arguments;
		Assert.Equal("A database error occurred", errorArgs.ViewModel.Title);
		Assert.Equal(
			"Fluent Feeds was unable to delete the selected item from the database.", errorArgs.ViewModel.Message);
	}
}
