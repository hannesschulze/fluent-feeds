using System;
using System.Collections.Immutable;
using System.Threading.Tasks;
using FluentFeeds.Common;
using FluentFeeds.Documents;
using FluentFeeds.Documents.Blocks;
using FluentFeeds.Documents.Inlines;
using FluentFeeds.Feeds.Base;
using FluentFeeds.Feeds.Base.Feeds;
using FluentFeeds.Feeds.Base.Feeds.Content;
using FluentFeeds.Feeds.Base.Items;
using FluentFeeds.Feeds.Base.Items.Content;

namespace FluentFeeds.App.Shared;

/// <summary>
/// Feed provider used for internal testing.
/// </summary>
public sealed class DummyFeedProvider : FeedProvider
{
	private sealed class ContentLoader : IFeedContentLoader
	{
		private const string LoremIpsum1 = "Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor invidunt ut labore et dolore magna aliquyam erat, sed diam voluptua. At vero eos et accusam et justo duo dolores et ea rebum. Stet clita kasd gubergren, no sea takimata sanctus est Lorem ipsum dolor sit amet. Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor invidunt ut labore et dolore magna aliquyam erat, sed diam voluptua. At vero eos et accusam et justo duo dolores et ea rebum. Stet clita kasd gubergren, no sea takimata sanctus est Lorem ipsum dolor sit amet. Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor invidunt ut labore et dolore magna aliquyam erat, sed diam voluptua. At vero eos et accusam et justo duo dolores et ea rebum. Stet clita kasd gubergren, no sea takimata sanctus est Lorem ipsum dolor sit amet.";
		private const string LoremIpsum2 = "Duis autem vel eum iriure dolor in hendrerit in vulputate velit esse molestie consequat, vel illum dolore eu feugiat nulla facilisis at vero eros et accumsan et iusto odio dignissim qui blandit praesent luptatum zzril delenit augue duis dolore te feugait nulla facilisi. Lorem ipsum dolor sit amet, consectetuer adipiscing elit, sed diam nonummy nibh euismod tincidunt ut laoreet dolore magna aliquam erat volutpat.";

		private static ItemContent CreateItemContent()
		{
			return new CommentItemContent(
				new Comment("foo", DateTimeOffset.Now - TimeSpan.FromMinutes(15), new RichText(new GenericBlock(new TextInline(LoremIpsum1))),
					new Comment("bar", DateTimeOffset.Now - TimeSpan.FromMinutes(10), new RichText(new GenericBlock(new TextInline("nice!")))),
					new Comment("baz", DateTimeOffset.Now - TimeSpan.FromMinutes(10), new RichText(new GenericBlock(new TextInline("that's cool!"))),
						new Comment("foobar", DateTimeOffset.Now, new RichText(new GenericBlock(new TextInline(LoremIpsum2)))))));
		}

		private static ImmutableArray<ItemDescriptor> CreateItems()
		{
			return ImmutableArray.Create(
				new ItemDescriptor(
					"item", "Test Item", null, null, DateTimeOffset.Now, DateTimeOffset.Now, null, null,
					new StaticItemContentLoader(CreateItemContent())));
		}

		public Task<FeedContent> LoadAsync()
		{
			return Task.FromResult(new FeedContent { Items = CreateItems() });
		}
	}
	
	public DummyFeedProvider() : base(new FeedProviderMetadata(
		Guid.Parse("4de529c1-3cbe-4f9d-bf6e-a8598ae91f77"), "Dummy", "Used for internal testing"))
	{
	}

	public override GroupFeedDescriptor CreateInitialTree()
	{
		return new GroupFeedDescriptor("Dummy feeds", Symbol.Feed,
			new CachedFeedDescriptor(new ContentLoader()) { Name = "Content" });
	}

	public override Task<string> StoreFeedAsync(IFeedContentLoader contentLoader)
	{
		return Task.FromResult("");
	}

	public override Task<IFeedContentLoader> LoadFeedAsync(string serialized)
	{
		return Task.FromResult<IFeedContentLoader>(new ContentLoader());
	}
}
