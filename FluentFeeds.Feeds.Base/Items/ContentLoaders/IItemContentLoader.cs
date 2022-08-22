using System.Threading.Tasks;
using FluentFeeds.Feeds.Base.Items.Content;

namespace FluentFeeds.Feeds.Base.Items.ContentLoaders;

/// <summary>
/// Abstraction for dynamically loading <see cref="ItemContent"/> objects when requested.
/// </summary>
public interface IItemContentLoader
{
	/// <summary>
	/// Asynchronously load the item content.
	/// </summary>
	/// <param name="reload">
	/// Flag indicating that no cached content should be returned and the content should instead be reloaded.
	/// </param>
	Task<ItemContent> LoadAsync(bool reload = false);
}
