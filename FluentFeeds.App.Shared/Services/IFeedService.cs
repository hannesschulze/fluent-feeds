using System.Collections.ObjectModel;
using System.Threading.Tasks;
using FluentFeeds.App.Shared.Models;

namespace FluentFeeds.App.Shared.Services;

/// <summary>
/// Service managing feed providers and the tree of feed nodes.
/// </summary>
public interface IFeedService
{
	/// <summary>
	/// Load the available feeds asynchronously from the local database.
	/// </summary>
	Task LoadFeedsAsync();
	
	/// <summary>
	/// A list of loaded feed providers, initially empty.
	/// </summary>
	ReadOnlyCollection<LoadedFeedProvider> FeedProviders { get; }
}
