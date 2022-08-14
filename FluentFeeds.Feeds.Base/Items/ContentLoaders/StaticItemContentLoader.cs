using System.Threading.Tasks;
using FluentFeeds.Feeds.Base.Items.Content;

namespace FluentFeeds.Feeds.Base.Items.ContentLoaders;

/// <summary>
/// Content loader implementation for static content.
/// </summary>
public sealed class StaticItemContentLoader : IItemContentLoader
{
	public StaticItemContentLoader(ItemContent content)
	{
		Content = content;
	}
	
	/// <summary>
	/// The static content.
	/// </summary>
	public ItemContent Content { get; }

	public Task<ItemContent> LoadAsync(bool reload = false) => Task.FromResult(Content);
}
