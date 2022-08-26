using System.Threading;
using System.Threading.Tasks;

namespace FluentFeeds.Feeds.Base.Items.Content;

/// <summary>
/// Simple loader implementation for static content.
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

	public Task<ItemContent> LoadAsync(bool reload = false, CancellationToken cancellation = default) =>
		Task.FromResult(Content);
}
