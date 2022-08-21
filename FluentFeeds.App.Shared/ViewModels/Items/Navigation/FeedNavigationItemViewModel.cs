using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using FluentFeeds.App.Shared.Helpers;
using FluentFeeds.App.Shared.Models;
using FluentFeeds.App.Shared.Services;
using FluentFeeds.App.Shared.ViewModels.Modals;
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
		if (FeedNode is not IReadOnlyStoredFeedNode || !FeedNode.IsUserCustomizable || FeedProvider == null)
			return null;

		var showGroupActions = FeedNode.Type == FeedNodeType.Group;
		var showEditActions = FeedNode != FeedProvider.RootNode;
		if (!showGroupActions && !showEditActions)
			return null;

		var result = new List<NavigationItemActionViewModel>();
		if (showGroupActions)
		{
			if (FeedProvider.Provider.UrlFeedFactory != null)
			{
				result.Add(new NavigationItemActionViewModel(new RelayCommand(HandleAddFeedCommand), "Add feed…", null));
			}
			result.Add(new NavigationItemActionViewModel(new RelayCommand(HandleAddGroupCommand), "Add group…", null));
		}
		if (showEditActions)
		{
			result.Add(new NavigationItemActionViewModel(new RelayCommand(HandleEditNodeCommand), "Edit…", null));
			result.Add(new NavigationItemActionViewModel(new RelayCommand(HandleDeleteNodeCommand), "Delete", null));
		}
		return result.ToImmutableArray();
	}

	public FeedNavigationItemViewModel(
		IFeedService feedService, IModalService modalService, IReadOnlyFeedNode feedNode,
		LoadedFeedProvider? feedProvider,
		Dictionary<IReadOnlyFeedNode, NavigationItemViewModel> feedItemRegistry) : base(
			NavigationRoute.Feed(feedNode), isExpandable: feedNode.Children != null, GetTitle(feedNode),
			GetSymbol(feedNode))
	{
		_feedService = feedService;
		_modalService = modalService;

		FeedNode = feedNode;
		FeedNode.PropertyChanged += HandlePropertyChanged;
		FeedProvider = feedProvider;
		Actions = GetActions();

		if (feedNode.Children != null)
		{
			ObservableCollectionTransformer.CreateCached(
				feedNode.Children, MutableChildren,
				node => new FeedNavigationItemViewModel(
					_feedService, _modalService, node, FeedProvider, feedItemRegistry),
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

	private void HandleAddFeedCommand()
	{
		var viewModel = new AddFeedViewModel(_feedService, FeedProvider!, (IReadOnlyStoredFeedNode)FeedNode);
		_modalService.ShowModal(viewModel, this);
	}

	private void HandleAddGroupCommand()
	{
		var viewModel = new AddGroupViewModel(_feedService, FeedProvider!, (IReadOnlyStoredFeedNode)FeedNode);
		_modalService.ShowModal(viewModel, this);
	}

	private void HandleEditNodeCommand()
	{
		var viewModel = new EditNodeViewModel(_feedService, FeedProvider!, (IReadOnlyStoredFeedNode)FeedNode);
		_modalService.ShowModal(viewModel, this);
	}

	private void HandleDeleteNodeCommand()
	{
		var viewModel = new DeleteNodeViewModel(_feedService, (IReadOnlyStoredFeedNode)FeedNode);
		_modalService.ShowModal(viewModel, this);
	}

	private readonly IFeedService _feedService;
	private readonly IModalService _modalService;
}
