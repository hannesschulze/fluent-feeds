using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentFeeds.App.Shared.EventArgs;
using FluentFeeds.App.Shared.Models.Feeds;
using FluentFeeds.App.Shared.Models.Feeds.Loaders;
using FluentFeeds.App.Shared.Models.Storage;
using FluentFeeds.Feeds.Base;
using FluentFeeds.Feeds.Base.Feeds;
using FluentFeeds.Feeds.Base.Feeds.Content;

namespace FluentFeeds.App.Shared.Tests.Mock;

public sealed class FeedStorageMock : IFeedStorage
{
	public FeedStorageMock(FeedProvider provider)
	{
		Provider = provider;
	}
	
	public FeedProvider Provider { get; }
	
	public bool DeleteFeedFails { get; set; }
	
	public event EventHandler<FeedsDeletedEventArgs>? FeedsDeleted;

	public IItemStorage GetItemStorage(Guid identifier)
	{
		return new ItemStorageMock(identifier);
	}

	public IFeedView? GetFeed(Guid identifier)
	{
		return _feeds.GetValueOrDefault(identifier);
	}

	private Feed CreateFeed(FeedDescriptor descriptor, IFeedView? parent)
	{
		var identifier = Guid.NewGuid();
		var feed = new Feed(
			identifier: identifier,
			storage: this,
			loaderFactory: descriptor switch
			{
				GroupFeedDescriptor => GroupFeedLoader.FromFeed,
				CachedFeedDescriptor cachedDescriptor =>
					_ => new CachedFeedLoader(
						identifier, GetItemStorage(cachedDescriptor.ItemCacheIdentifier ?? identifier),
						cachedDescriptor.ContentLoader),
				_ => throw new IndexOutOfRangeException()
			},
			hasChildren: descriptor.Type == FeedDescriptorType.Group,
			parent: parent,
			name: descriptor.Name,
			symbol: descriptor.Symbol,
			metadata: new FeedMetadata(),
			isUserCustomizable: descriptor.IsUserCustomizable,
			isExcludedFromGroup: descriptor.IsExcludedFromGroup);
		if (feed.Children != null && descriptor is GroupFeedDescriptor groupFeedDescriptor)
		{
			foreach (var child in groupFeedDescriptor.Children)
			{
				feed.Children.Add(CreateFeed(child, feed));
			}
		}
		
		_feeds.Add(identifier, feed);

		return feed;
	}

	public IFeedView AddRootNode(FeedDescriptor descriptor)
	{
		return CreateFeed(descriptor, null);
	}

	public Task<IFeedView> AddFeedAsync(FeedDescriptor descriptor, Guid parentIdentifier, bool syncFirst = false)
	{
		var parent = _feeds[parentIdentifier];
		var feed = CreateFeed(descriptor, parent);
		parent.Children!.Add(feed);
		return Task.FromResult<IFeedView>(feed);
	}

	public Task<IFeedView> RenameFeedAsync(Guid identifier, string newName)
	{
		var feed = _feeds[identifier];
		feed.Name = newName;
		return Task.FromResult<IFeedView>(feed);
	}

	public Task<IFeedView> UpdateFeedMetadataAsync(Guid identifier, FeedMetadata newMetadata)
	{
		var feed = _feeds[identifier];
		feed.Metadata = newMetadata;
		return Task.FromResult<IFeedView>(feed);
	}

	public Task<IFeedView> MoveFeedAsync(Guid identifier, Guid newParentIdentifier)
	{
		var feed = _feeds[identifier];
		var newParent = _feeds[newParentIdentifier];
		(feed.Parent as Feed)?.Children!.Remove(feed);
		feed.Parent = newParent;
		newParent.Children!.Add(feed);
		return Task.FromResult<IFeedView>(feed);
	}

	public Task DeleteFeedAsync(Guid identifier)
	{
		if (DeleteFeedFails)
			throw new Exception("error");
		
		var node = _feeds[identifier];
		(node.Parent as Feed)?.Children!.Remove(node);
		FeedsDeleted?.Invoke(this, new FeedsDeletedEventArgs(new[] { node }));
		_feeds.Remove(identifier);
		return Task.CompletedTask;
	}

	private readonly Dictionary<Guid, Feed> _feeds = new();
}
