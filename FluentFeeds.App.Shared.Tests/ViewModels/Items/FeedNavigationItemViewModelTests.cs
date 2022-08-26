using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using FluentFeeds.App.Shared.Models.Feeds;
using FluentFeeds.App.Shared.Models.Feeds.Loaders;
using FluentFeeds.App.Shared.Models.Navigation;
using FluentFeeds.App.Shared.Tests.Mock;
using FluentFeeds.App.Shared.ViewModels.Items.Navigation;
using FluentFeeds.Common;
using FluentFeeds.Feeds.Base.Feeds;
using FluentFeeds.Feeds.Base.Feeds.Content;
using Xunit;

namespace FluentFeeds.App.Shared.Tests.ViewModels.Items;

public class FeedNavigationItemViewModelTests
{
	public FeedNavigationItemViewModelTests()
	{
		Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture; 
	}
	
	private ModalServiceMock ModalService { get; } = new();

	[Fact]
	public void FeedWithChildren()
	{
		var feed = new Feed(
			identifier: Guid.Empty,
			storage: null,
			loaderFactory: _ => new EmptyFeedLoader(),
			hasChildren: true,
			parent: null,
			name: "Title",
			symbol: Symbol.Directory,
			metadata: new FeedMetadata(),
			isUserCustomizable: true,
			isExcludedFromGroup: false);
		var item = new FeedNavigationItemViewModel(
			ModalService, feed, null, new Dictionary<IFeedView, NavigationItemViewModel>());
		Assert.Equal("Title",item.Title);
		Assert.Equal(Symbol.Directory, item.Symbol);
		Assert.Equal(MainNavigationRoute.Feed(feed), item.Destination);
		Assert.True(item.IsExpandable);
		Assert.Empty(item.Children);

		var childFeed = new Feed(
			identifier: Guid.Empty,
			storage: null,
			loaderFactory: _ => new EmptyFeedLoader(),
			hasChildren: true,
			parent: feed,
			name: "Title",
			symbol: Symbol.Directory,
			metadata: new FeedMetadata(),
			isUserCustomizable: true,
			isExcludedFromGroup: false);
		feed.Children!.Add(childFeed);
		var childItem = Assert.IsType<FeedNavigationItemViewModel>(Assert.Single(item.Children));
		Assert.Equal(childFeed, childItem.Feed);
	}
	
	[Fact]
	public void FeedWithoutChildren()
	{
		var feed = new Feed(
			identifier: Guid.Empty,
			storage: null,
			loaderFactory: _ => new EmptyFeedLoader(),
			hasChildren: false,
			parent: null,
			name: null,
			symbol: null,
			metadata: new FeedMetadata { Name = "Feed", Symbol = Symbol.Web },
			isUserCustomizable: true,
			isExcludedFromGroup: false);
		var item = new FeedNavigationItemViewModel(
			ModalService, feed, null, new Dictionary<IFeedView, NavigationItemViewModel>());
		Assert.Equal("Feed",item.Title);
		Assert.Equal(Symbol.Web, item.Symbol);
		Assert.Equal(MainNavigationRoute.Feed(feed), item.Destination);
		Assert.False(item.IsExpandable);
		Assert.Empty(item.Children);
	}

	[Fact]
	public void AvailableActions_NodeIsNotStored()
	{
		var feed = new Feed(
			identifier: Guid.Empty,
			storage: null,
			loaderFactory: _ => new EmptyFeedLoader(),
			hasChildren: false,
			parent: null,
			name: null,
			symbol: null,
			metadata: new FeedMetadata(),
			isUserCustomizable: true,
			isExcludedFromGroup: false);
		var item = new FeedNavigationItemViewModel(
			ModalService, feed, null, new Dictionary<IFeedView, NavigationItemViewModel>());
		Assert.Empty(item.Actions);
	}

	[Fact]
	public void AvailableActions_RootNode_UserCustomizable_WithUrlFactory()
	{
		var feedStorage = new FeedStorageMock(new FeedProviderMock(Guid.Empty));
		var node = feedStorage.AddRootNode(new GroupFeedDescriptor());
		var item = new FeedNavigationItemViewModel(
			ModalService, node, node, new Dictionary<IFeedView, NavigationItemViewModel>());
		Assert.Collection(
			item.Actions,
			action => Assert.Equal("Add feed…", action.Title),
			action => Assert.Equal("Add group…", action.Title));
	}

	[Fact]
	public void AvailableActions_RootNode_UserCustomizable_WithoutUrlFactory()
	{
		var feedStorage = new FeedStorageMock(new FeedProviderMock(Guid.Empty, hasUrlFactory: false));
		var node = feedStorage.AddRootNode(new GroupFeedDescriptor());
		var item = new FeedNavigationItemViewModel(
			ModalService, node, node, new Dictionary<IFeedView, NavigationItemViewModel>());
		Assert.Collection(
			item.Actions,
			action => Assert.Equal("Add group…", action.Title));
	}

	[Fact]
	public void AvailableActions_RootNode_NotUserCustomizable()
	{
		var feedStorage = new FeedStorageMock(new FeedProviderMock(Guid.Empty));
		var node = feedStorage.AddRootNode(new GroupFeedDescriptor { IsUserCustomizable = false });
		var item = new FeedNavigationItemViewModel(
			ModalService, node, node, new Dictionary<IFeedView, NavigationItemViewModel>());
		Assert.Empty(item.Actions);
	}

	[Fact]
	public void AvailableActions_GroupNode_UserCustomizable_WithUrlFactory()
	{
		var feedStorage = new FeedStorageMock(new FeedProviderMock(Guid.Empty));
		var root = feedStorage.AddRootNode(new GroupFeedDescriptor());
		var node = feedStorage.AddFeedAsync(new GroupFeedDescriptor(), root.Identifier).Result;
		var item = new FeedNavigationItemViewModel(
			ModalService, node, root, new Dictionary<IFeedView, NavigationItemViewModel>());
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
		var root = feedStorage.AddRootNode(new GroupFeedDescriptor());
		var node = feedStorage.AddFeedAsync(new GroupFeedDescriptor(), root.Identifier).Result;
		var item = new FeedNavigationItemViewModel(
			ModalService, node, root, new Dictionary<IFeedView, NavigationItemViewModel>());
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
		var root = feedStorage.AddRootNode(new GroupFeedDescriptor());
		var node = feedStorage.AddFeedAsync(
			new GroupFeedDescriptor { IsUserCustomizable = false }, root.Identifier).Result;
		var item = new FeedNavigationItemViewModel(
			ModalService, node, root, new Dictionary<IFeedView, NavigationItemViewModel>());
		Assert.Empty(item.Actions);
	}

	[Fact]
	public void AvailableActions_LeafNode_UserCustomizable()
	{
		var feedStorage = new FeedStorageMock(new FeedProviderMock(Guid.Empty));
		var root = feedStorage.AddRootNode(new GroupFeedDescriptor());
		var node = feedStorage.AddFeedAsync(
			new CachedFeedDescriptor(new FeedContentLoaderMock("")), root.Identifier).Result;
		var item = new FeedNavigationItemViewModel(
			ModalService, node, root, new Dictionary<IFeedView, NavigationItemViewModel>());
		Assert.Collection(
			item.Actions,
			action => Assert.Equal("Edit…", action.Title),
			action => Assert.Equal("Delete", action.Title));
	}

	[Fact]
	public void AvailableActions_LeafNode_NotUserCustomizable()
	{
		var feedStorage = new FeedStorageMock(new FeedProviderMock(Guid.Empty));
		var root = feedStorage.AddRootNode(new GroupFeedDescriptor());
		var node = feedStorage.AddFeedAsync(
			new CachedFeedDescriptor(new FeedContentLoaderMock("")) { IsUserCustomizable = false },
			root.Identifier).Result;
		var item = new FeedNavigationItemViewModel(
			ModalService, node, root, new Dictionary<IFeedView, NavigationItemViewModel>());
		Assert.Empty(item.Actions);
	}

	[Fact]
	public void UpdateProperties()
	{
		var feedStorage = new FeedStorageMock(new FeedProviderMock(Guid.Empty));
		var root = feedStorage.AddRootNode(new GroupFeedDescriptor());
		var feed = feedStorage.AddFeedAsync(
			new CachedFeedDescriptor(new FeedContentLoaderMock("")), root.Identifier).Result;
		var item = new FeedNavigationItemViewModel(
			ModalService, feed, root, new Dictionary<IFeedView, NavigationItemViewModel>());
		feedStorage.UpdateFeedMetadataAsync(
			feed.Identifier, new FeedMetadata { Name = "Updated", Symbol = Symbol.Web });
		((Feed)feed).IsUserCustomizable = false;
		Assert.Equal("Updated", item.Title);
		Assert.Equal(Symbol.Web, item.Symbol);
		Assert.Empty(item.Actions);
	}

	[Fact]
	public async Task Actions_AddFeed()
	{
		var feedStorage = new FeedStorageMock(new FeedProviderMock(Guid.Empty, hasUrlFactory: true));
		var root = feedStorage.AddRootNode(new GroupFeedDescriptor("root", null));
		var group = await feedStorage.AddFeedAsync(new GroupFeedDescriptor("group", null), root.Identifier);
		var item = new FeedNavigationItemViewModel(
			ModalService, group, root, new Dictionary<IFeedView, NavigationItemViewModel>());
		var modalArgs = Assert.Raises<ModalServiceMock.ShowFeedDataModalEventArgs>(
			h => ModalService.ShowFeedDataModal += h, h => ModalService.ShowFeedDataModal -= h,
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
		var loader = Assert.IsType<CachedFeedLoader>(node.Loader);
		var contentLoader = Assert.IsType<FeedContentLoaderMock>(loader.ContentLoader);
		Assert.Equal("https://www.example.com/", contentLoader.Identifier);
	}

	[Fact]
	public async Task Actions_AddGroup()
	{
		var feedStorage = new FeedStorageMock(new FeedProviderMock(Guid.Empty, hasUrlFactory: false));
		var root = feedStorage.AddRootNode(new GroupFeedDescriptor("root", null));
		var group = await feedStorage.AddFeedAsync(new GroupFeedDescriptor("group", null), root.Identifier);
		var item = new FeedNavigationItemViewModel(
			ModalService, group, root, new Dictionary<IFeedView, NavigationItemViewModel>());
		var modalArgs = Assert.Raises<ModalServiceMock.ShowFeedDataModalEventArgs>(
			h => ModalService.ShowFeedDataModal += h, h => ModalService.ShowFeedDataModal -= h,
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
		Assert.NotNull(node.Children);
		Assert.Equal("test", node.Name);
	}

	[Fact]
	public async Task Actions_EditNode()
	{
		var feedStorage = new FeedStorageMock(new FeedProviderMock(Guid.Empty, hasUrlFactory: false));
		var root = feedStorage.AddRootNode(new GroupFeedDescriptor("root", null));
		var group = await feedStorage.AddFeedAsync(new GroupFeedDescriptor("group", null), root.Identifier);
		var node = await feedStorage.AddFeedAsync(new GroupFeedDescriptor("feed", null), group.Identifier);
		var item = new FeedNavigationItemViewModel(
			ModalService, node, root, new Dictionary<IFeedView, NavigationItemViewModel>());
		var modalArgs = Assert.Raises<ModalServiceMock.ShowFeedDataModalEventArgs>(
			h => ModalService.ShowFeedDataModal += h, h => ModalService.ShowFeedDataModal -= h,
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
				Assert.Equal("feed", groupItem.Title);
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
		Assert.Equal("test", node.Name);
	}

	[Fact]
	public async Task Actions_DeleteNode_Success()
	{
		var feedStorage = new FeedStorageMock(new FeedProviderMock(Guid.Empty, hasUrlFactory: false));
		var root = feedStorage.AddRootNode(new GroupFeedDescriptor("root", null));
		var node = await feedStorage.AddFeedAsync(new GroupFeedDescriptor("feed", null), root.Identifier);
		var item = new FeedNavigationItemViewModel(
			ModalService, node, root, new Dictionary<IFeedView, NavigationItemViewModel>());
		var modalArgs = Assert.Raises<ModalServiceMock.ShowDeleteFeedModalEventArgs>(
			h => ModalService.ShowDeleteFeedModal += h, h => ModalService.ShowDeleteFeedModal -= h,
			() => item.Actions[2].Command.Execute(null)).Arguments;
		Assert.Equal(item, modalArgs.RelatedItem);
		
		modalArgs.ViewModel.ConfirmCommand.Execute(null);
		Assert.Empty(root.Children!);
	}

	[Fact]
	public async Task Actions_DeleteNode_Error()
	{
		var feedStorage = new FeedStorageMock(new FeedProviderMock(Guid.Empty, hasUrlFactory: false));
		var root = feedStorage.AddRootNode(new GroupFeedDescriptor("root", null));
		var node = await feedStorage.AddFeedAsync(new GroupFeedDescriptor("feed", null), root.Identifier);
		var item = new FeedNavigationItemViewModel(
			ModalService, node, root, new Dictionary<IFeedView, NavigationItemViewModel>());
		var modalArgs = Assert.Raises<ModalServiceMock.ShowDeleteFeedModalEventArgs>(
			h => ModalService.ShowDeleteFeedModal += h, h => ModalService.ShowDeleteFeedModal -= h,
			() => item.Actions[2].Command.Execute(null)).Arguments;
		Assert.Equal(item, modalArgs.RelatedItem);

		feedStorage.DeleteFeedFails = true;
		var errorArgs = Assert.Raises<ModalServiceMock.ShowErrorModalEventArgs>(
			h => ModalService.ShowErrorModal += h, h => ModalService.ShowErrorModal -= h,
			() => modalArgs.ViewModel.ConfirmCommand.Execute(null)).Arguments;
		Assert.Equal("A database error occurred", errorArgs.ViewModel.Title);
		Assert.Equal(
			"Fluent Feeds was unable to delete the selected item from the database.", errorArgs.ViewModel.Message);
	}
}
