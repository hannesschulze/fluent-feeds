using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using FluentFeeds.App.Shared.Helpers;
using FluentFeeds.App.Shared.Models;
using FluentFeeds.Common;
using FluentFeeds.Feeds.Base.Nodes;
using Microsoft.Toolkit.Mvvm.Input;

namespace FluentFeeds.App.Shared.ViewModels.Main;

/// <summary>
/// List item representing a feed on the main page.
/// </summary>
public sealed class MainFeedItemViewModel : MainItemViewModel
{
	private static string GetTitle(IReadOnlyFeedNode feedNode)
	{
		return feedNode.ActualTitle ?? "Unnamed";
	}

	private static Symbol GetSymbol(IReadOnlyFeedNode feedNode)
	{
		return feedNode.ActualSymbol ?? Symbol.Feed;
	}

	private static ImmutableArray<MainItemActionViewModel>? GetActions(
		IReadOnlyFeedNode feedNode, LoadedFeedProvider? provider)
	{
		if (feedNode is not IReadOnlyStoredFeedNode storedFeedNode || !feedNode.IsUserCustomizable || provider == null)
			return null;

		var result = new List<MainItemActionViewModel>();
		if (feedNode.Type == FeedNodeType.Group)
		{
			if (provider.Provider.UrlFeedFactory != null)
			{
				result.Add(new MainItemActionViewModel(new RelayCommand(() => { }), "Add feed…", null));
			}
			result.Add(new MainItemActionViewModel(new RelayCommand(() => { }), "Add group…", null));
		}
		result.Add(new MainItemActionViewModel(new RelayCommand(() => { }), "Rename…", null));
		result.Add(new MainItemActionViewModel(new RelayCommand(() => { }), "Move…", null));
		result.Add(new MainItemActionViewModel(new RelayCommand(() => { }), "Delete", null));
		return result.ToImmutableArray();
	}

	public MainFeedItemViewModel(
		IReadOnlyFeedNode feedNode, LoadedFeedProvider? feedProvider, 
		Dictionary<IReadOnlyFeedNode, MainItemViewModel> feedItemRegistry) : base(
			NavigationRoute.Feed(feedNode), isExpandable: feedNode.Children != null, GetTitle(feedNode),
			GetSymbol(feedNode), GetActions(feedNode, feedProvider))
	{
		FeedNode = feedNode;
		FeedNode.PropertyChanged += HandlePropertyChanged;
		FeedProvider = feedProvider;
		
		if (feedNode.Children != null)
		{
			ObservableCollectionTransformer.CreateCached(
				feedNode.Children, Children,
				node => new MainFeedItemViewModel(node, FeedProvider, feedItemRegistry), feedItemRegistry);
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
				Title = GetTitle(FeedNode);
				break;
			case nameof(IReadOnlyFeedNode.ActualSymbol):
				Symbol = GetSymbol(FeedNode);
				break;
			case nameof(IReadOnlyFeedNode.IsUserCustomizable):
				Actions = GetActions(FeedNode, FeedProvider);
				break;
		}
	}
}
