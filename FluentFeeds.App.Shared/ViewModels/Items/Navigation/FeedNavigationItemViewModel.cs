using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using FluentFeeds.App.Shared.Helpers;
using FluentFeeds.App.Shared.Models;
using FluentFeeds.Common;
using FluentFeeds.Feeds.Base.Nodes;
using Microsoft.Toolkit.Mvvm.Input;

namespace FluentFeeds.App.Shared.ViewModels.Items.Navigation;

/// <summary>
/// Navigation item representing a feed.
/// </summary>
public sealed class FeedNavigationItemViewModel : NavigationItemViewModel
{
	private static string GetTitle(IReadOnlyFeedNode feedNode)
	{
		return feedNode.ActualTitle ?? "Unnamed";
	}

	private static Symbol GetSymbol(IReadOnlyFeedNode feedNode)
	{
		return feedNode.ActualSymbol ?? Symbol.Feed;
	}

	private ImmutableArray<NavigationItemActionViewModel>? GetActions()
	{
		if (FeedNode is not IReadOnlyStoredFeedNode storedNode || !FeedNode.IsUserCustomizable || FeedProvider == null)
			return null;

		var result = new List<NavigationItemActionViewModel>();
		if (FeedNode.Type == FeedNodeType.Group)
		{
			if (FeedProvider.Provider.UrlFeedFactory != null)
			{
				result.Add(new NavigationItemActionViewModel(new RelayCommand(() => { }), "Add feed…", null));
			}
			result.Add(new NavigationItemActionViewModel(new RelayCommand(() => { }), "Add group…", null));
		}
		result.Add(new NavigationItemActionViewModel(new RelayCommand(() => { }), "Rename…", null));
		result.Add(new NavigationItemActionViewModel(new RelayCommand(() => { }), "Move…", null));
		result.Add(new NavigationItemActionViewModel(new RelayCommand(() => { }), "Delete…", null));
		return result.ToImmutableArray();
	}

	public FeedNavigationItemViewModel(
		IReadOnlyFeedNode feedNode, LoadedFeedProvider? feedProvider, 
		Dictionary<IReadOnlyFeedNode, NavigationItemViewModel> feedItemRegistry) : base(
			NavigationRoute.Feed(feedNode), isExpandable: feedNode.Children != null, GetTitle(feedNode),
			GetSymbol(feedNode))
	{
		FeedNode = feedNode;
		FeedNode.PropertyChanged += HandlePropertyChanged;
		FeedProvider = feedProvider;

		Actions = GetActions();
		if (feedNode.Children != null)
		{
			ObservableCollectionTransformer.CreateCached(
				feedNode.Children, MutableChildren,
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
				Title = GetTitle(FeedNode);
				break;
			case nameof(IReadOnlyFeedNode.ActualSymbol):
				Symbol = GetSymbol(FeedNode);
				break;
			case nameof(IReadOnlyFeedNode.IsUserCustomizable):
				Actions = GetActions();
				break;
		}
	}
}
