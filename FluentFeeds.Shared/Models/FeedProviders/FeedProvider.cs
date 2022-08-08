using System;
using FluentFeeds.Shared.Models.Nodes;

namespace FluentFeeds.Shared.Models.FeedProviders;

public abstract class FeedProvider
{
	public abstract FeedGroup CreateInitialTree();
	public abstract FeedItem CreateItem(Uri url);
	public abstract FeedItem LoadItem(string serialized);
}
