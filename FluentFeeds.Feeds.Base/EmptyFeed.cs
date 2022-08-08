using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentFeeds.Feeds.Base.Items;

namespace FluentFeeds.Feeds.Base;

/// <summary>
/// A feed implementation not containing any items.
/// </summary>
public sealed class EmptyFeed : Feed
{
	protected override Task<IEnumerable<IReadOnlyItem>> DoLoadAsync() =>
		Task.FromResult(Enumerable.Empty<IReadOnlyItem>());

	protected override Task<IEnumerable<IReadOnlyItem>> DoSynchronizeAsync() =>
		Task.FromResult(Enumerable.Empty<IReadOnlyItem>());
}
