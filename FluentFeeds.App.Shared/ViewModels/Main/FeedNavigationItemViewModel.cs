using System.Collections.Generic;
using System.ComponentModel;
using FluentFeeds.App.Shared.Helpers;
using FluentFeeds.App.Shared.Models;
using FluentFeeds.Common;
using FluentFeeds.Feeds.Base.Nodes;

namespace FluentFeeds.App.Shared.ViewModels.Main;

/// <summary>
/// Navigation item representing a feed node.
/// </summary>
public sealed class FeedNavigationItemViewModel : NavigationItemViewModel
{
	public FeedNavigationItemViewModel(
		IReadOnlyFeedNode feedNode, LoadedFeedProvider? feedProvider,
		Dictionary<IReadOnlyFeedNode, NavigationItemViewModel> feedItemRegistry) : base(
		feedNode.ActualTitle ?? "Unnamed", feedNode.ActualSymbol ?? Symbol.Feed,
		isExpandable: feedNode.Children != null, NavigationRoute.Feed(feedNode))
	{
		FeedNode = feedNode;
		FeedNode.PropertyChanged += HandlePropertyChanged;
		FeedProvider = feedProvider;
		
		if (feedNode.Children != null)
		{
			ObservableCollectionTransformer.CreateCached(
				feedNode.Children, Children,
				node => new FeedNavigationItemViewModel(node, FeedProvider, feedItemRegistry), feedItemRegistry);
		}
	}
	
	/// <summary>
	/// The source feed node.
	/// </summary>
	public IReadOnlyFeedNode FeedNode { get; }
	
	/// <summary>
	/// Feed provider to which the feed belongs.
	/// </summary>
	public LoadedFeedProvider? FeedProvider { get; }

	private void HandlePropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		switch (e.PropertyName)
		{
			case nameof(IReadOnlyFeedNode.ActualTitle):
				Title = FeedNode.ActualTitle ?? "Unnamed";
				break;
			case nameof(IReadOnlyFeedNode.ActualSymbol):
				Symbol = FeedNode.ActualSymbol ?? Symbol.Feed;
				break;
		}
	}
}
