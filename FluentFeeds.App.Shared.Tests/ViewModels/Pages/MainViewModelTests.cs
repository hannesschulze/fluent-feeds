using System;
using System.Threading.Tasks;
using FluentFeeds.App.Shared.Models.Navigation;
using FluentFeeds.App.Shared.Tests.Mock;
using FluentFeeds.App.Shared.ViewModels.Items.Navigation;
using FluentFeeds.App.Shared.ViewModels.Pages;
using FluentFeeds.Common;
using FluentFeeds.Feeds.Base.Nodes;
using Xunit;

namespace FluentFeeds.App.Shared.Tests.ViewModels.Pages;

public class MainViewModelTests
{
	public MainViewModelTests()
	{
		ModalService = new ModalServiceMock();
		FeedService = new FeedServiceMock();
	}
	
	private ModalServiceMock ModalService { get; }
	private FeedServiceMock FeedService { get; }

	[Fact]
	public void ManagesNavigationItems_Initialization_Success()
	{
		var viewModel = new MainViewModel(FeedService, ModalService);
		var provider = new FeedProviderMock(Guid.Empty);
		var feedStorage = new FeedStorageMock(provider);
		var node = feedStorage.AddRootNode(provider.CreateInitialTree(feedStorage));
		FeedService.ProviderNodes.Add(node);
		FeedService.CompleteInitialization();

		Assert.Collection(
			viewModel.FeedItems,
			item =>
			{
				var feedItem = Assert.IsType<FeedNavigationItemViewModel>(item);
				Assert.Equal(FeedService.OverviewNode, feedItem.FeedNode);
				Assert.Null(feedItem.RootNode);
			},
			item =>
			{
				var feedItem = Assert.IsType<FeedNavigationItemViewModel>(item);
				Assert.Equal("Unread", feedItem.Title);
				Assert.Null(feedItem.RootNode);
			},
			item =>
			{
				var feedItem = Assert.IsType<FeedNavigationItemViewModel>(item);
				Assert.Equal(node, feedItem.FeedNode);
				Assert.Equal(node, feedItem.RootNode);
			});

		Assert.Collection(
			viewModel.FooterItems,
			item =>
			{
				Assert.Equal("Settings", item.Title);
				Assert.Equal(Symbol.Settings, item.Symbol);
				Assert.Equal(MainNavigationRoute.Settings, item.Destination);
				Assert.False(item.IsExpandable);
				Assert.Empty(item.Children);
				Assert.Empty(item.Actions);
			});
	}

	[Fact]
	public void ManagesNavigationItems_Initialization_Error()
	{
		_ = new MainViewModel(FeedService, ModalService);
		var modal = Assert.Raises<ModalServiceMock.ShowErrorModalEventArgs>(
			h => ModalService.ShowErrorModal += h, h => ModalService.ShowErrorModal -= h,
			() => FeedService.CompleteInitialization(new Exception("error"))).Arguments;
		Assert.Equal("A database error occurred", modal.ViewModel.Title);
		Assert.Equal("Fluent Feeds was unable to initialize its database.", modal.ViewModel.Message);
	}

	[Fact]
	public void Navigation_UsingItem()
	{
		var viewModel = new MainViewModel(FeedService, ModalService);
		FeedService.CompleteInitialization();
		
		Assert.Equal(MainNavigationRoute.Feed(FeedService.OverviewNode), viewModel.CurrentRoute);
		Assert.Equal(viewModel.FeedItems[0], viewModel.SelectedItem);
		Assert.False(viewModel.GoBackCommand.CanExecute(null));
		viewModel.SelectedItem = viewModel.FooterItems[0];
		Assert.Equal(MainNavigationRoute.Settings, viewModel.CurrentRoute);
		Assert.Equal(viewModel.FooterItems[0], viewModel.SelectedItem);
		Assert.True(viewModel.GoBackCommand.CanExecute(null));
	}

	[Fact]
	public void Navigation_UsingBackButton()
	{
		var viewModel = new MainViewModel(FeedService, ModalService);
		FeedService.CompleteInitialization();
		
		viewModel.SelectedItem = viewModel.FooterItems[0];
		viewModel.GoBackCommand.Execute(null);
		Assert.Equal(MainNavigationRoute.Feed(FeedService.OverviewNode), viewModel.CurrentRoute);
		Assert.Equal(viewModel.FeedItems[0], viewModel.SelectedItem);
		Assert.False(viewModel.GoBackCommand.CanExecute(null));
	}

	[Fact]
	public async Task Navigation_RemoveDeletedNodes()
	{
		var viewModel = new MainViewModel(FeedService, ModalService);
		var provider = new FeedProviderMock(Guid.Empty);
		var feedStorage = new FeedStorageMock(provider);
		var rootNode = feedStorage.AddRootNode(FeedNode.Group("root", Symbol.Directory, true));
		var nodeA = await feedStorage.AddNodeAsync(FeedNode.Group("a", Symbol.Directory, true), rootNode.Identifier);
		var nodeB = await feedStorage.AddNodeAsync(FeedNode.Group("b", Symbol.Directory, true), rootNode.Identifier);
		FeedService.ProviderNodes.Add(rootNode);
		FeedService.CompleteInitialization();

		viewModel.SelectedItem = viewModel.FooterItems[0];
		viewModel.SelectedItem = viewModel.FeedItems[2].Children[0];
		viewModel.SelectedItem = viewModel.FeedItems[2].Children[1];
		viewModel.SelectedItem = viewModel.FeedItems[2].Children[0];
		viewModel.SelectedItem = viewModel.FeedItems[2].Children[1];
		
		await feedStorage.DeleteNodeAsync(nodeB.Identifier);
		Assert.Equal(MainNavigationRoute.Feed(nodeA), viewModel.CurrentRoute);
		Assert.Equal(viewModel.FeedItems[2].Children[0], viewModel.SelectedItem);
		Assert.True(viewModel.GoBackCommand.CanExecute(null));
		viewModel.GoBackCommand.Execute(null);
		Assert.Equal(MainNavigationRoute.Settings, viewModel.CurrentRoute);
		Assert.Equal(viewModel.FooterItems[0], viewModel.SelectedItem);
		Assert.True(viewModel.GoBackCommand.CanExecute(null));
		viewModel.GoBackCommand.Execute(null);
		Assert.Equal(MainNavigationRoute.Feed(FeedService.OverviewNode), viewModel.CurrentRoute);
		Assert.Equal(viewModel.FeedItems[0], viewModel.SelectedItem);
		Assert.False(viewModel.GoBackCommand.CanExecute(null));
	}
}
