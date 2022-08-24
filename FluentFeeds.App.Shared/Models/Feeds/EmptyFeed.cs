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
	protected override Task DoLoadAsync() => Task.CompletedTask;

	protected override Task DoSynchronizeAsync() => Task.CompletedTask;
}
