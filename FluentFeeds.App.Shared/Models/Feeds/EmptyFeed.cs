using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentFeeds.App.Shared.Models.Items;

namespace FluentFeeds.App.Shared.Models.Feeds;

/// <summary>
/// A feed implementation not containing any items.
/// </summary>
public sealed class EmptyFeed : Feed
{
	protected override Task<IEnumerable<Item>> DoLoadAsync() => Task.FromResult(Enumerable.Empty<Item>());

	protected override Task<IEnumerable<Item>> DoSynchronizeAsync() => Task.FromResult(Enumerable.Empty<Item>());
}
