using System.Threading.Tasks;

namespace FluentFeeds.Feeds.Base.Items.Content;

/// <summary>
/// An interface which can be used for dynamically loading an item's content when requested.
/// </summary>
public interface IItemContentLoader
{
	/// <summary>
	/// Asynchronously load the item's content.
	/// </summary>
	/// <param name="reload">
	/// Flag indicating that no cached content should be returned and the content should instead be reloaded.
	/// </param>
	Task<ItemContent> LoadAsync(bool reload = false);
}
