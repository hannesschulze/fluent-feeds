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
	Task InitializeAsync();

	/// <summary>
	/// The root nodes for loaded feed providers.
	/// </summary>
	ReadOnlyObservableCollection<IReadOnlyStoredFeedNode> ProviderNodes { get; }
	
	/// <summary>
	/// Feed node providing an overview of all available feeds.
	/// </summary>
	IReadOnlyFeedNode OverviewNode { get; }
}
