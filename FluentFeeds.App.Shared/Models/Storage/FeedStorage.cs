using System;
using System.Threading.Tasks;
using FluentFeeds.App.Shared.EventArgs;
using FluentFeeds.App.Shared.Models.Feeds;
using FluentFeeds.Feeds.Base;
using FluentFeeds.Feeds.Base.Feeds;
using FluentFeeds.Feeds.Base.Feeds.Content;

namespace FluentFeeds.App.Shared.Models.Storage;

public class FeedStorage : IFeedStorage
{
	public FeedStorage(FeedProvider provider)
	{
		Provider = provider;
	}
	
	public FeedProvider Provider { get; }
	
	public event EventHandler<FeedsDeletedEventArgs>? FeedsDeleted;
	
	public IItemStorage GetItemStorage(Guid identifier)
	{
		throw new NotImplementedException();
	}

	public IFeedView? GetFeed(Guid identifier)
	{
		throw new NotImplementedException();
	}

	public Task<IFeedView> AddFeedAsync(FeedDescriptor descriptor, Guid parentIdentifier)
	{
		throw new NotImplementedException();
	}

	public Task<IFeedView> RenameFeedAsync(Guid identifier, string newTitle)
	{
		throw new NotImplementedException();
	}

	public Task<IFeedView> MoveFeedAsync(Guid identifier, Guid newParentIdentifier)
	{
		throw new NotImplementedException();
	}

	public Task<IFeedView> UpdateFeedMetadataAsync(Guid identifier, FeedMetadata newMetadata)
	{
		throw new NotImplementedException();
	}

	public Task DeleteFeedAsync(Guid identifier)
	{
		throw new NotImplementedException();
	}
}
