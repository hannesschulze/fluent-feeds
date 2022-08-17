using System.Collections.Generic;
using System.ComponentModel;
using FluentFeeds.App.Shared.Helpers;
using FluentFeeds.App.Shared.Models;
using FluentFeeds.Common;
using FluentFeeds.Feeds.Base.Nodes;

namespace FluentFeeds.App.Shared.ViewModels.Main;

/// <summary>
/// List item representing a feed on the main page.
/// </summary>
public sealed class MainFeedItemViewModel : MainNavigationItemViewModel
{
	public MainFeedItemViewModel(
		IReadOnlyFeedNode feedNode, LoadedFeedProvider? feedProvider, 
		Dictionary<IReadOnlyFeedNode, MainItemViewModel> feedItemRegistry) : base(
		feedNode.ActualTitle ?? "Unnamed", feedNode.ActualSymbol ?? Symbol.Feed,
		isExpandable: feedNode.Children != null, NavigationRoute.Feed(feedNode))
	{
		FeedNode = feedNode;
		FeedNode.PropertyChanged += HandlePropertyChanged;
		FeedProvider = feedProvider;
		
		if (feedNode.Children != null)
		{
			ObservableCollectionTransformer.CreateCached(
				feedNode.Children, Children, node => new MainFeedItemViewModel(node, FeedProvider, feedItemRegistry),
				feedItemRegistry);
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
