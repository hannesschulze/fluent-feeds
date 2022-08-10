using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentFeeds.Feeds.Base;
using FluentFeeds.Feeds.Base.Items;
using FluentFeeds.Feeds.Base.Storage;

namespace FluentFeeds.Feeds.Syndication;

public sealed class SyndicationFeed : CachedFeed
{
	public SyndicationFeed(IItemStorage storage, Guid collectionIdentifier) : base(storage, collectionIdentifier)
	{
	}

	protected override Task<IEnumerable<IReadOnlyItem>> DoFetchAsync()
	{
		throw new NotImplementedException();
	}
}
