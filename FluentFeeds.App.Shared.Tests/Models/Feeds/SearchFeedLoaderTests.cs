using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using FluentFeeds.App.Shared.Models.Feeds.Loaders;
using FluentFeeds.App.Shared.Models.Items;
using FluentFeeds.App.Shared.Tests.Mock;
using FluentFeeds.Documents;
using FluentFeeds.Feeds.Base.Items;
using FluentFeeds.Feeds.Base.Items.Content;
using Xunit;

namespace FluentFeeds.App.Shared.Tests.Models.Feeds;

public class SearchFeedLoaderTests
{
	private ItemStorageMock ItemStorageMock { get; } = new();
	
	private IItemView CreateDummyItem(string title, string? author, string? summary)
	{
		return ItemStorageMock.AddItems(
			new[]
			{
				new ItemDescriptor(
					null, title, author, summary, DateTimeOffset.Now, DateTimeOffset.Now, null, null,
					new StaticItemContentLoader(new ArticleItemContent(new RichText())))
			}, Guid.Empty).First();
	}

	[Fact]
	public void ForwardItemsWithoutSearchTerms()
	{
		var source = new FeedLoaderMock();
		var itemA = CreateDummyItem("item a", null, null);
		var itemB = CreateDummyItem("item b", null, null);
		var itemC = CreateDummyItem("item c", null, null);
		source.SynchronizeAsync();
		source.CompleteInitialize();
		source.CompleteSynchronize(itemA, itemB);
		var loader = new SearchFeedLoader(source);
		Assert.NotNull(loader.LastSynchronized);
		Assert.Equal(2, loader.Items.Count);
		Assert.Contains(itemA, loader.Items);
		Assert.Contains(itemB, loader.Items);
		loader.SynchronizeAsync();
		source.CompleteSynchronize(itemA, itemC);
		Assert.Equal(2, loader.Items.Count);
		Assert.Contains(itemA, loader.Items);
		Assert.Contains(itemC, loader.Items);
	}

	private static Task WaitForItemUpdate(FeedLoader loader, Action action)
	{
		var completionSource = new TaskCompletionSource();
		loader.ItemsUpdated += (s, e) => completionSource.TrySetResult();
		action.Invoke();
		return completionSource.Task;
	}

	[Fact]
	public async Task Search()
	{
		var source = new FeedLoaderMock();
		var itemA = CreateDummyItem("FFooo", "BBarr", null);
		var itemB = CreateDummyItem("BBarr", "bbaZZ", "FFOOO");
		var itemC = CreateDummyItem("bbArr", "BBAZZ", null);
		source.UpdateItems(itemA, itemC);
		var loader = new SearchFeedLoader(source);
		await WaitForItemUpdate(loader, () => loader.SearchTerms = ImmutableArray.Create("foo", "bar"));
		Assert.Single(loader.Items);
		Assert.Contains(itemA, loader.Items);
		await WaitForItemUpdate(loader, () => source.UpdateItems(itemA, itemB, itemC));
		Assert.Equal(2, loader.Items.Count);
		Assert.Contains(itemA, loader.Items);
		Assert.Contains(itemB, loader.Items);
	}
}
