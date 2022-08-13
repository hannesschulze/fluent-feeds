using System.Collections.ObjectModel;
using System.Threading.Tasks;
using FluentFeeds.App.Shared.Models;
using FluentFeeds.Feeds.Base.Nodes;

namespace FluentFeeds.App.Shared.Services;

/// <summary>
/// Service managing feed providers and the tree of feed nodes.
/// </summary>
public interface IFeedService
{
	/// <summary>
	/// Load the available feeds asynchronously from the local database.
	/// </summary>
	Task LoadFeedProvidersAsync();
	
	/// <summary>
	/// A list of loaded feed providers, initially empty.
	/// </summary>
	ReadOnlyObservableCollection<LoadedFeedProvider> FeedProviders { get; }
	
	/// <summary>
	/// Feed node providing an overview of all available feeds.
	/// </summary>
	IReadOnlyFeedNode OverviewFeed { get; }
}
