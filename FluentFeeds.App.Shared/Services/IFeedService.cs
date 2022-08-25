using System.Collections.ObjectModel;
using System.Threading.Tasks;
using FluentFeeds.App.Shared.Models.Feeds;

namespace FluentFeeds.App.Shared.Services;

/// <summary>
/// Service managing feed providers and the tree of feed nodes.
/// </summary>
public interface IFeedService
{
	/// <summary>
	/// Load the available feeds asynchronously from the local database.
	/// </summary>
	Task InitializeAsync();

	/// <summary>
	/// The root feeds for loaded feed providers.
	/// </summary>
	ReadOnlyObservableCollection<IFeedView> ProviderFeeds { get; }
	
	/// <summary>
	/// Feed providing an overview of all available feeds.
	/// </summary>
	IFeedView OverviewFeed { get; }
}
