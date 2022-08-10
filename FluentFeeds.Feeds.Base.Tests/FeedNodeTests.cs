using FluentFeeds.Common;
using FluentFeeds.Feeds.Base.Nodes;
using Xunit;

namespace FluentFeeds.Feeds.Base.Tests;

public class FeedNodeTests
{
	[Fact]
	public void CustomNode()
	{
		var feed = new EmptyFeed();
		var node = FeedNode.Custom(feed, "feed", Symbol.Feed, false);
		Assert.Equal(FeedNodeType.Custom, node.Type);
		Assert.Equal(feed, node.Feed);
		Assert.Equal("feed", node.Title);
		Assert.Equal(Symbol.Feed, node.Symbol);
		Assert.False(node.IsUserCustomizable);
		Assert.Null(node.Children);
		Assert.Null((node as IReadOnlyFeedNode).Children);
	}

	[Fact]
	public void GroupNode()
	{
		var feedA = new EmptyFeed();
		var nodeA = FeedNode.Custom(feedA, null, null, false);
		var feedB = new EmptyFeed();
		var nodeB = FeedNode.Custom(feedB, null, null, false);
		var node = FeedNode.Group("feed", Symbol.Directory, true, nodeA);
		Assert.Equal(FeedNodeType.Group, node.Type);
		Assert.Equal("feed", node.Title);
		Assert.Equal(Symbol.Directory, node.Symbol);
		Assert.True(node.IsUserCustomizable);
		Assert.NotNull(node.Children);
		Assert.NotNull((node as IReadOnlyFeedNode).Children);
		Assert.IsType<CompositeFeed>(node.Feed);
		
		Assert.Collection(
			node.Children!,
			item => Assert.Equal(nodeA, item));
		Assert.Collection(
			(node as IReadOnlyFeedNode).Children!,
			item => Assert.Equal(nodeA, item));
		Assert.Single(((CompositeFeed)node.Feed).Feeds);
		node.Children!.Add(nodeB);
		Assert.Collection(
			node.Children!,
			item => Assert.Equal(nodeA, item),
			item => Assert.Equal(nodeB, item));
		Assert.Collection(
			(node as IReadOnlyFeedNode).Children!,
			item => Assert.Equal(nodeA, item),
			item => Assert.Equal(nodeB, item));
		Assert.Equal(2, ((CompositeFeed)node.Feed).Feeds.Count);
	}
}
