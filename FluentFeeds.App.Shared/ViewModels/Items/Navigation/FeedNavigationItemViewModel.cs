using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using FluentFeeds.App.Shared.Helpers;
using FluentFeeds.App.Shared.Models;
using FluentFeeds.App.Shared.Services;
using FluentFeeds.App.Shared.ViewModels.Modals;
using FluentFeeds.Feeds.Base.Nodes;
using Microsoft.Toolkit.Mvvm.Input;

namespace FluentFeeds.App.Shared.ViewModels.Items.Navigation;

/// <summary>
/// Navigation item representing a feed.
/// </summary>
public sealed class FeedNavigationItemViewModel : NavigationItemViewModel
{
	private ImmutableArray<NavigationItemActionViewModel> BuildActions()
	{
		if (FeedNode is not IReadOnlyStoredFeedNode storedNode || !FeedNode.IsUserCustomizable || _rootNode == null)
			return ImmutableArray<NavigationItemActionViewModel>.Empty;

		var result = new List<NavigationItemActionViewModel>();
		
		if (storedNode.Type == FeedNodeType.Group)
		{
			var urlFactory = storedNode.Storage.Provider.UrlFeedFactory;
			if (urlFactory != null)
			{
				result.Add(new NavigationItemActionViewModel(
					new RelayCommand(() =>
						_modalService.Show(new AddFeedViewModel(urlFactory, _rootNode, storedNode), this)),
					"Add feed…", null));
			}
			result.Add(new NavigationItemActionViewModel(
				new RelayCommand(() => _modalService.Show(new AddGroupViewModel(_rootNode, storedNode), this)),
				"Add group…", null));
		}
		
		if (storedNode != _rootNode)
		{
			result.Add(new NavigationItemActionViewModel(
				new RelayCommand(() => _modalService.Show(new EditNodeViewModel(_rootNode, storedNode), this)),
				"Edit…", null));
			result.Add(new NavigationItemActionViewModel(
				new RelayCommand(() => _modalService.Show(new DeleteNodeViewModel(_modalService, storedNode), this)),
				"Delete", null));
		}
		
		return result.ToImmutableArray();
	}

	public FeedNavigationItemViewModel(
		IModalService modalService, IReadOnlyFeedNode feedNode, IReadOnlyStoredFeedNode? rootNode,
		Dictionary<IReadOnlyFeedNode, NavigationItemViewModel> feedItemRegistry) : base(
			NavigationRoute.Feed(feedNode), isExpandable: feedNode.Children != null, feedNode.DisplayTitle,
			feedNode.DisplaySymbol)
	{
		_modalService = modalService;
		_rootNode = rootNode;

		FeedNode = feedNode;
		FeedNode.PropertyChanged += HandlePropertyChanged;
		Actions = BuildActions();

		if (feedNode.Children != null)
		{
			ObservableCollectionTransformer.CreateCached(
				feedNode.Children, MutableChildren,
				node => new FeedNavigationItemViewModel(modalService, node, rootNode, feedItemRegistry),
				feedItemRegistry);
		}
	}
	
	/// <summary>
	/// The source feed node.
	/// </summary>
	public IReadOnlyFeedNode FeedNode { get; }

	private void HandlePropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		switch (e.PropertyName)
		{
			case nameof(IReadOnlyFeedNode.DisplayTitle):
				Title = FeedNode.DisplayTitle;
				break;
			case nameof(IReadOnlyFeedNode.DisplaySymbol):
				Symbol = FeedNode.DisplaySymbol;
				break;
			case nameof(IReadOnlyFeedNode.IsUserCustomizable):
				Actions = BuildActions();
				break;
		}
	}

	private readonly IModalService _modalService;
	private readonly IReadOnlyStoredFeedNode? _rootNode;
}
