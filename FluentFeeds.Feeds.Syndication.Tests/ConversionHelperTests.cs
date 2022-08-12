using System;
using System.IO;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Threading.Tasks;
using System.Xml;
using FluentFeeds.Documents;
using FluentFeeds.Documents.Blocks;
using FluentFeeds.Documents.Inlines;
using FluentFeeds.Feeds.Base.Items.Content;
using FluentFeeds.Feeds.Syndication.Helpers;
using Xunit;
using SysSyndicationFeed = System.ServiceModel.Syndication.SyndicationFeed;

namespace FluentFeeds.Feeds.Syndication.Tests;

public class ConversionHelperTests
{
	[Fact]
	public void ConvertAuthor_Missing()
	{
		var source = Enumerable.Empty<SyndicationPerson>();
		var author = ConversionHelpers.ConvertAuthor(source);
		Assert.Null(author);
	}
	
	[Fact]
	public void ConvertAuthor_Single()
	{
		var source = new[] { new SyndicationPerson { Name = "John Doe" } };
		var author = ConversionHelpers.ConvertAuthor(source);
		Assert.Equal("John Doe", author);
	}
	
	[Fact]
	public void ConvertAuthor_Multiple()
	{
		var source =
			new[]
			{
				new SyndicationPerson { Name = "John Doe" },
				new SyndicationPerson(),
				new SyndicationPerson { Name = "Jane Doe" }
			};
		var author = ConversionHelpers.ConvertAuthor(source);
		Assert.Equal("John Doe, Jane Doe", author);
	}

	[Fact]
	public void ConvertItemUrl_UseAlternate()
	{
		var source =
			new SyndicationItem
			{
				Links =
				{
					new SyndicationLink(new Uri("https://test-blog")) { RelationshipType = "related" },
					new SyndicationLink(new Uri("https://www.example.com")) { RelationshipType = "alternate" },
					new SyndicationLink(new Uri("https://other-blog")) { RelationshipType = "self" }
				}
			};
		var url = ConversionHelpers.ConvertItemUrl(source);
		Assert.Equal(new Uri("https://www.example.com"), url);
	}

	[Fact]
	public void ConvertItemUrl_MissingAlternate()
	{
		var source =
			new SyndicationItem
			{
				Links =
				{
					new SyndicationLink(new Uri("https://www.example.com")) { RelationshipType = "related" },
					new SyndicationLink(new Uri("https://other-blog")) { RelationshipType = "self" }
				}
			};
		var url = ConversionHelpers.ConvertItemUrl(source);
		Assert.Equal(new Uri("https://www.example.com"), url);
	}

	[Fact]
	public void ConvertItemUrl_Empty()
	{
		var source = new SyndicationItem();
		var url = ConversionHelpers.ConvertItemUrl(source);
		Assert.Null(url);
	}

	[Fact]
	public async Task ConvertItemContent_DefaultContent()
	{
		var source =
			new SyndicationItem
			{
				Content = new TextSyndicationContent("<p>Test <b>content</b></p>", TextSyndicationContentKind.Plaintext)
			};
		var content = await ConversionHelpers.ConvertItemContentAsync(source, null);
		var expected = new RichText(new ParagraphBlock(
			new TextInline("Test "), new BoldInline(new TextInline("content"))));
		Assert.Equal(expected, content);
	}

	[Fact]
	public async Task ConvertItemContent_EncodedContent()
	{
		const string input =
			"<?xml version=\"1.0\" encoding=\"utf-8\"?>\n" +
			"<rss version=\"2.0\" xmlns:content=\"http://purl.org/rss/1.0/modules/content/\">\n" +
			"  <channel>\n" +
			"    <item>\n" +
			"      <content:encoded>&lt;p&gt;Test &lt;b&gt;content&lt;/b&gt;&lt;/p&gt;</content:encoded>\n" +
			"    </item>\n" +
			"  </channel>\n" +
			"</rss>";
		using var reader = XmlReader.Create(new StringReader(input));
		var source = SysSyndicationFeed.Load(reader).Items.First();
		var content = await ConversionHelpers.ConvertItemContentAsync(source, null);
		var expected = new RichText(new ParagraphBlock(
			new TextInline("Test "), new BoldInline(new TextInline("content"))));
		Assert.Equal(expected, content);
	}

	[Fact]
	public async Task ConvertItemContent_SummaryAsFallback()
	{
		var source = new SyndicationItem();
		var summary = await TextContent.LoadAsync("<p>Test <b>summary</b></p>");
		var content = await ConversionHelpers.ConvertItemContentAsync(source, summary);
		var expected = new RichText(new ParagraphBlock(
			new TextInline("Test "), new BoldInline(new TextInline("summary"))));
		Assert.Equal(expected, content);
	}

	[Fact]
	public async Task ConvertItemContent_Missing()
	{
		var source = new SyndicationItem();
		var content = await ConversionHelpers.ConvertItemContentAsync(source, null);
		Assert.Equal(new RichText(), content);
	}

	[Fact]
	public async Task ConvertItem_Title_Missing()
	{
		var source = new SyndicationItem();
		var item = await ConversionHelpers.ConvertItemAsync(source);
		Assert.Empty(item.Title);
	}

	[Fact]
	public async Task ConvertItem_Title_Available()
	{
		var source =
			new SyndicationItem
			{
				Title = new TextSyndicationContent("Test <b>item</b>", TextSyndicationContentKind.Html)
			};
		var item = await ConversionHelpers.ConvertItemAsync(source);
		Assert.Equal("Test item", item.Title);
	}

	[Fact]
	public async Task ConvertItem_Author_Missing()
	{
		var source = new SyndicationItem();
		var item = await ConversionHelpers.ConvertItemAsync(source);
		Assert.Null(item.Author);
	}

	[Fact]
	public async Task ConvertItem_Author_Available()
	{
		var source = new SyndicationItem { Authors = { new SyndicationPerson { Name = "John Doe" } } };
		var item = await ConversionHelpers.ConvertItemAsync(source);
		Assert.Equal("John Doe", item.Author);
	}

	[Fact]
	public async Task ConvertItem_Summary_AutomaticallyGenerated()
	{
		var source =
			new SyndicationItem
			{
				Content = new TextSyndicationContent("Test <b>content</b>", TextSyndicationContentKind.Html)
			};
		var item = await ConversionHelpers.ConvertItemAsync(source);
		Assert.Equal("Test content", item.Summary);
	}

	[Fact]
	public async Task ConvertItem_Summary_ExplicitlySpecified()
	{
		var source =
			new SyndicationItem
			{
				Summary = new TextSyndicationContent("Test <b>summary</b>", TextSyndicationContentKind.Html)
			};
		var item = await ConversionHelpers.ConvertItemAsync(source);
		Assert.Equal("Test summary", item.Summary);
	}

	[Fact]
	public async Task ConvertItem_Content()
	{
		var source =
			new SyndicationItem
			{
				Content = new TextSyndicationContent("Test <b>content</b>", TextSyndicationContentKind.Html)
			};
		var item = await ConversionHelpers.ConvertItemAsync(source);
		var expected = new ArticleItemContent(new RichText(
			new GenericBlock(new TextInline("Test "), new BoldInline(new TextInline("content")))));
		Assert.Equal(expected, item.Content);
	}

	[Fact]
	public async Task ConvertItem_Url()
	{
		var source = new SyndicationItem { Links = { new SyndicationLink(new Uri("https://www.example.com/")) } };
		var item = await ConversionHelpers.ConvertItemAsync(source);
		Assert.Equal(new Uri("https://www.example.com/"), item.Url);
	}

	[Fact]
	public async Task ConvertItem_Timestamps()
	{
		var published = DateTimeOffset.Now - TimeSpan.FromDays(1);
		var modified = DateTimeOffset.Now;
		var source = new SyndicationItem { PublishDate = published, LastUpdatedTime = modified };
		var item = await ConversionHelpers.ConvertItemAsync(source);
		Assert.Equal(published, item.PublishedTimestamp);
		Assert.Equal(modified, item.ModifiedTimestamp);
	}

	[Fact]
	public async Task ConvertFeedMetadata_Author_Missing()
	{
		var source = new SysSyndicationFeed();
		var metadata = await ConversionHelpers.ConvertFeedMetadataAsync(source);
		Assert.Null(metadata.Author);
	}

	[Fact]
	public async Task ConvertFeedMetadata_Author_Available()
	{
		var source = new SysSyndicationFeed { Authors = { new SyndicationPerson { Name = "John Doe" } } };
		var metadata = await ConversionHelpers.ConvertFeedMetadataAsync(source);
		Assert.Equal("John Doe", metadata.Author);
	}

	[Fact]
	public async Task ConvertFeedMetadata_Name_Missing()
	{
		var source = new SysSyndicationFeed();
		var metadata = await ConversionHelpers.ConvertFeedMetadataAsync(source);
		Assert.Null(metadata.Name);
	}

	[Fact]
	public async Task ConvertFeedMetadata_Name_Available()
	{
		var source =
			new SysSyndicationFeed
			{
				Title = new TextSyndicationContent("My <b>blog</b>", TextSyndicationContentKind.Html)
			};
		var metadata = await ConversionHelpers.ConvertFeedMetadataAsync(source);
		Assert.Equal("My blog", metadata.Name);
	}

	[Fact]
	public async Task ConvertFeedMetadata_Description_Missing()
	{
		var source = new SysSyndicationFeed();
		var metadata = await ConversionHelpers.ConvertFeedMetadataAsync(source);
		Assert.Null(metadata.Description);
	}

	[Fact]
	public async Task ConvertFeedMetadata_Description_Available()
	{
		var source =
			new SysSyndicationFeed
			{
				Description = new TextSyndicationContent("Blog <b>description</b>", TextSyndicationContentKind.Html)
			};
		var metadata = await ConversionHelpers.ConvertFeedMetadataAsync(source);
		Assert.Equal("Blog description", metadata.Description);
	}
}
