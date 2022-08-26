using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using FluentFeeds.App.Shared.Helpers;
using FluentFeeds.App.Shared.Models.Feeds;
using FluentFeeds.App.Shared.Models.Navigation;
using FluentFeeds.App.Shared.Resources;
using FluentFeeds.App.Shared.Services;
using FluentFeeds.App.Shared.ViewModels.Modals;
using Microsoft.Toolkit.Mvvm.Input;

namespace FluentFeeds.App.Shared.ViewModels.Items.Navigation;

/// <summary>
/// Navigation item representing a feed.
/// </summary>
public sealed class FeedNavigationItemViewModel : NavigationItemViewModel
{
	private ImmutableArray<NavigationItemActionViewModel> BuildActions()
	{
		var storage = Feed.Storage;
		if (storage == null || !Feed.IsUserCustomizable || RootFeed == null)
			return ImmutableArray<NavigationItemActionViewModel>.Empty;

		var result = new List<NavigationItemActionViewModel>();
		
		if (Feed.Children != null)
		{
			var urlFactory = storage.Provider.UrlFeedFactory;
			if (urlFactory != null)
			{
				result.Add(new NavigationItemActionViewModel(
					new RelayCommand(() => _modalService.Show(
						new AddFeedViewModel(urlFactory, RootFeed, Feed, storage), this)),
					LocalizedStrings.FeedActionAddFeed, null));
			}
			result.Add(new NavigationItemActionViewModel(
				new RelayCommand(() => _modalService.Show(new AddGroupViewModel(RootFeed, Feed, storage), this)),
				LocalizedStrings.FeedActionAddGroup, null));
		}
		
		if (Feed != RootFeed)
		{
			result.Add(new NavigationItemActionViewModel(
				new RelayCommand(() => _modalService.Show(new EditFeedViewModel(RootFeed, Feed, storage), this)),
				LocalizedStrings.FeedActionEdit, null));
			result.Add(new NavigationItemActionViewModel(
				new RelayCommand(() => _modalService.Show(new DeleteFeedViewModel(_modalService, Feed, storage), this)),
				LocalizedStrings.FeedActionDelete, null));
		}
		
		return result.ToImmutableArray();
	}

	public FeedNavigationItemViewModel(
		IModalService modalService, IFeedView feed, IFeedView? rootFeed,
		Dictionary<IFeedView, NavigationItemViewModel> feedItemRegistry) : base(
			MainNavigationRoute.Feed(feed), isExpandable: feed.Children != null, feed.DisplayName, feed.DisplaySymbol)
	{
		_modalService = modalService;

		Feed = feed;
		Feed.PropertyChanged += HandlePropertyChanged;
		RootFeed = rootFeed;
		Actions = BuildActions();

		if (feed.Children != null)
		{
			ObservableCollectionTransformer.CreateCached(
				feed.Children, MutableChildren,
				child => new FeedNavigationItemViewModel(modalService, child, rootFeed, feedItemRegistry), 
				feedItemRegistry);
		}
	}
	
	/// <summary>
	/// The source feed.
	/// </summary>
	public IFeedView Feed { get; }
	
	/// <summary>
	/// The root for the source feed.
	/// </summary>
	public IFeedView? RootFeed { get; }

	private void HandlePropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		switch (e.PropertyName)
		{
			case nameof(IFeedView.DisplayName):
				Title = Feed.DisplayName;
				break;
			case nameof(IFeedView.DisplaySymbol):
				Symbol = Feed.DisplaySymbol;
				break;
			case nameof(IFeedView.IsUserCustomizable):
				Actions = BuildActions();
				break;
		}
	}

	private readonly IModalService _modalService;
}
