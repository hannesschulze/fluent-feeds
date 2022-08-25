using System.Threading.Tasks;

namespace FluentFeeds.Feeds.Base.Feeds.Content;

/// <summary>
/// An interface which can be used to fetch the updated content for a cached feed. 
/// </summary>
public interface IFeedContentLoader
{
	/// <summary>
	/// Asynchronously load the feed's updated content.
	/// </summary>
	Task<FeedContent> LoadAsync();
}
