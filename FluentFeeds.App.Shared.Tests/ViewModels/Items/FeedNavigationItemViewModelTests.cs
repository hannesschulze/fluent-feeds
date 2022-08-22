using System;
using System.Collections.Generic;
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
	public void GroupNode()
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
	public void LeafNode()
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
	public void UpdateProperties()
	{
		var feed = new FeedMock(Guid.Empty);
		feed.UpdateMetadata(new FeedMetadata { Name = "Feed", Symbol = Symbol.Web });
		var node = FeedNode.Custom(feed, null, null, isUserCustomizable: true);
		var item = new FeedNavigationItemViewModel(
			ModalService, node, null, new Dictionary<IReadOnlyFeedNode, NavigationItemViewModel>());
		feed.UpdateMetadata(new FeedMetadata { Name = "Updated", Symbol = Symbol.Feed });
		Assert.Equal("Updated",item.Title);
		Assert.Equal(Symbol.Feed, item.Symbol);
	}
}
