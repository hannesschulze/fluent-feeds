using System;
using System.Collections.Immutable;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using FluentFeeds.App.Shared.Models.Feeds.Loaders;
using FluentFeeds.App.Shared.Models.Navigation;
using FluentFeeds.App.Shared.Tests.Mock;
using FluentFeeds.App.Shared.ViewModels.Items.Navigation;
using FluentFeeds.App.Shared.ViewModels.Pages;
using FluentFeeds.Common;
using FluentFeeds.Feeds.Base.Feeds;
using Xunit;

namespace FluentFeeds.App.Shared.Tests.ViewModels.Pages;

public class MainViewModelTests
{
	public MainViewModelTests()
	{
		Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
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
		var node = feedStorage.AddRootNode(provider.CreateInitialTree());
		FeedService.ProviderFeeds.Add(node);
		FeedService.CompleteInitialization();

		Assert.Collection(
			viewModel.FeedItems,
			item =>
			{
				var feedItem = Assert.IsType<FeedNavigationItemViewModel>(item);
				Assert.Equal(FeedService.OverviewFeed, feedItem.Feed);
				Assert.Null(feedItem.RootFeed);
			},
			item =>
			{
				var feedItem = Assert.IsType<FeedNavigationItemViewModel>(item);
				Assert.Equal(node, feedItem.Feed);
				Assert.Equal(node, feedItem.RootFeed);
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
		
		Assert.Equal(MainNavigationRoute.Feed(FeedService.OverviewFeed), viewModel.CurrentRoute);
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
		Assert.Equal(MainNavigationRoute.Feed(FeedService.OverviewFeed), viewModel.CurrentRoute);
		Assert.Equal(viewModel.FeedItems[0], viewModel.SelectedItem);
		Assert.False(viewModel.GoBackCommand.CanExecute(null));
	}

	[Fact]
	public async Task Navigation_RemoveDeletedNodes()
	{
		var viewModel = new MainViewModel(FeedService, ModalService);
		var provider = new FeedProviderMock(Guid.Empty);
		var feedStorage = new FeedStorageMock(provider);
		var rootNode = feedStorage.AddRootNode(new GroupFeedDescriptor("root", Symbol.Directory));
		var nodeA = await feedStorage.AddFeedAsync(new GroupFeedDescriptor("a", Symbol.Directory), rootNode.Identifier);
		var nodeB = await feedStorage.AddFeedAsync(new GroupFeedDescriptor("b", Symbol.Directory), rootNode.Identifier);
		FeedService.ProviderFeeds.Add(rootNode);
		FeedService.CompleteInitialization();

		viewModel.SelectedItem = viewModel.FooterItems[0];
		viewModel.SelectedItem = viewModel.FeedItems[1].Children[0];
		viewModel.SelectedItem = viewModel.FeedItems[1].Children[1];
		viewModel.SelectedItem = viewModel.FeedItems[1].Children[0];
		viewModel.SelectedItem = viewModel.FeedItems[1].Children[1];
		
		await feedStorage.DeleteFeedAsync(nodeB.Identifier);
		Assert.Equal(MainNavigationRoute.Feed(nodeA), viewModel.CurrentRoute);
		Assert.Equal(viewModel.FeedItems[1].Children[0], viewModel.SelectedItem);
		Assert.True(viewModel.GoBackCommand.CanExecute(null));
		viewModel.GoBackCommand.Execute(null);
		Assert.Equal(MainNavigationRoute.Settings, viewModel.CurrentRoute);
		Assert.Equal(viewModel.FooterItems[0], viewModel.SelectedItem);
		Assert.True(viewModel.GoBackCommand.CanExecute(null));
		viewModel.GoBackCommand.Execute(null);
		Assert.Equal(MainNavigationRoute.Feed(FeedService.OverviewFeed), viewModel.CurrentRoute);
		Assert.Equal(viewModel.FeedItems[0], viewModel.SelectedItem);
		Assert.False(viewModel.GoBackCommand.CanExecute(null));
	}

	[Fact]
	public void Search()
	{
		var viewModel = new MainViewModel(FeedService, ModalService) { HasBuggySelection = false };
		FeedService.CompleteInitialization();

		viewModel.SearchText = " foo   bar ";
		viewModel.SearchCommand.Execute(null);
		Assert.Equal(2, viewModel.FeedItems.Count);
		var searchFeed = Assert.IsType<FeedNavigationItemViewModel>(viewModel.FeedItems[1]).Feed;
		Assert.Equal(viewModel.FeedItems[1], viewModel.SelectedItem);
		Assert.Equal(MainNavigationRoute.Feed(searchFeed), viewModel.CurrentRoute);
		Assert.Equal("Search", searchFeed.Name);
		Assert.Equal(Symbol.Search, searchFeed.Symbol);
		var searchLoader = Assert.IsType<SearchFeedLoader>(searchFeed.Loader);
		Assert.Equal(new[] { "foo", "bar" }, searchLoader.SearchTerms);
		viewModel.SearchText = "baz";
		viewModel.SearchCommand.Execute(null);
		Assert.Equal(new[] { "baz" }, searchLoader.SearchTerms);
		viewModel.SearchText = "";
		Assert.Equal(ImmutableArray<string>.Empty, searchLoader.SearchTerms);
		Assert.Single(viewModel.FeedItems);
		Assert.Equal(viewModel.FeedItems[0], viewModel.SelectedItem);
		Assert.Equal(MainNavigationRoute.Feed(FeedService.OverviewFeed), viewModel.CurrentRoute);
	}
}
