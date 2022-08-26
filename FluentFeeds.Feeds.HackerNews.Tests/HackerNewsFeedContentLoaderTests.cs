using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using FluentFeeds.Feeds.HackerNews.Models;
using FluentFeeds.Feeds.HackerNews.Tests.Mock;
using Xunit;

namespace FluentFeeds.Feeds.HackerNews.Tests;

public class HackerNewsFeedContentLoaderTests
{
	public HackerNewsFeedContentLoaderTests()
	{
		Downloader =
			new DownloaderMock
			{
				ItemListResponse = new ItemListResponse(ImmutableArray.Create<long>(8863, 121003)),
				ItemResponse =
					new Dictionary<long, ItemResponse> 
					{
						[8863] =
							new(8863, "story")
							{
								By = "dhouston",
								Time = 1175714200,
								Title = "My YC app: Dropbox - Throw away your USB drive",
								Url = "http://www.getdropbox.com/u/2/screencast.html"
							},
						[121003] =
							new(121003, "story")
							{
								By = "tel",
								Text =
									"<i>or</i> HN: the Next Iteration<p>I get the impression that with Arc being " +
									"released a lot of people who never had time for HN before are suddenly dropping " +
									"in more often. (PG: what are the numbers on this? I'm envisioning a spike.)<p>" +
									"Not to say that isn't great, but I'm wary of Diggification. Between links " +
									"comparing programming to sex and a flurry of gratuitous, ostentatious  " +
									"adjectives in the headlines it's a bit concerning.<p>80% of the stuff that " +
									"makes the front page is still pretty awesome, but what's in place to keep the " +
									"signal/noise ratio high? Does the HN model still work as the community scales? " +
									"What's in store for (++ HN)?",
								Time = 1203647620,
								Title = "Ask HN: The Arc Effect"
							}
					}
			};
		ContentLoader = new HackerNewsFeedContentLoader(Downloader, HackerNewsFeedType.Top);
	}
	
	private DownloaderMock Downloader { get; }
	private HackerNewsFeedContentLoader ContentLoader { get; }
	
	[Fact]
	public async Task Load()
	{
		var content = await ContentLoader.LoadAsync();
		Assert.Collection(
			content.Items.OrderBy(i => i.Title),
			item => Assert.Equal("Ask HN: The Arc Effect", item.Title),
			item => Assert.Equal("My YC app: Dropbox - Throw away your USB drive", item.Title));
	}
}
