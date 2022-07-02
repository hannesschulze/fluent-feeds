using System;
using FluentFeeds.Shared.RichText;
using FluentFeeds.Shared.RichText.Blocks;
using FluentFeeds.Shared.RichText.Blocks.Heading;
using FluentFeeds.Shared.RichText.Blocks.List;
using FluentFeeds.Shared.RichText.Inlines;
using FluentFeeds.Shared.Services;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace FluentFeeds.WinUI.Pages;

public sealed partial class FeedPage : Page
{
	public FeedPage()
	{
		InitializeComponent();
		_richText = new RichText(
			new ParagraphBlock(
				new TextInline("This is a demo for the "),
				new HyperlinkInline(
					new TextInline("FluentFeeds "),
					/*new HyperlinkInline(
						new TextInline("Rich Text"))
					{ Target = new Uri("https://github.com/") },*/
					new TextInline(" object model"))
				{ Target = new Uri("https://www.example.com") },
				new TextInline(".")),
			new HorizontalRuleBlock(),
			new HeadingBlock(
				new TextInline("A heading at "),
				new CodeInline(
					new TextInline("Level 1")))
			{ Level = HeadingLevel.Level1 },
			new HeadingBlock(
				new TextInline("A heading at "),
				new CodeInline(
					new TextInline("Level 2")))
			{ Level = HeadingLevel.Level2 },
			new HeadingBlock(
				new TextInline("A heading at "),
				new CodeInline(
					new TextInline("Level 3")))
			{ Level = HeadingLevel.Level3 },
			new HeadingBlock(
				new TextInline("A heading at "),
				new CodeInline(
					new TextInline("Level 4")))
			{ Level = HeadingLevel.Level4 },
			new HeadingBlock(
				new TextInline("A heading at "),
				new CodeInline(
					new TextInline("Level 5")))
			{ Level = HeadingLevel.Level5 },
			new HeadingBlock(
				new TextInline("A heading at "),
				new CodeInline(
					new TextInline("Level 6")))
			{ Level = HeadingLevel.Level6 },
			new HorizontalRuleBlock(),
			new HeadingBlock(
				new TextInline("Formatting Text"))
			{ Level = HeadingLevel.Level2 },
			new ParagraphBlock(
				new TextInline("This is "),
				new BoldInline(
					new TextInline("bold text")),
				new TextInline(".")),
			new ParagraphBlock(
				new TextInline("This is "),
				new ItalicInline(
					new TextInline("italic text")),
				new TextInline(".")),
			new ParagraphBlock(
				new TextInline("This is "),
				new UnderlineInline(
					new TextInline("underlined text")),
				new TextInline(".")),
			new ParagraphBlock(
				new TextInline("This is "),
				new StrikethroughInline(
					new TextInline("strikethrough text")),
				new TextInline(".")),
			new HeadingBlock(
				new TextInline("Blockquotes"))
			{ Level = HeadingLevel.Level2 },
			new QuoteBlock(
				new ParagraphBlock(
					new TextInline(
						"Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor " +
						"invidunt ut labore et dolore magna aliquyam erat, sed diam voluptua. At vero eos et accusam " +
						"et justo duo dolores et ea rebum. Stet clita kasd gubergren, no sea takimata sanctus est " +
						"Lorem ipsum dolor sit amet. Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed " +
						"diam nonumy eirmod tempor invidunt ut labore et dolore magna aliquyam erat, sed diam " +
						"voluptua. At vero eos et accusam et justo duo dolores et ea rebum. Stet clita kasd " +
						"gubergren, no sea takimata sanctus est Lorem ipsum dolor sit amet. Lorem ipsum dolor sit " +
						"amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor invidunt ut labore et " +
						"dolore magna aliquyam erat, sed diam voluptua. At vero eos et accusam et justo duo dolores " +
						"et ea rebum. Stet clita kasd gubergren, no sea takimata sanctus est Lorem ipsum dolor sit " +
						"amet.")),
				new QuoteBlock(
					new ParagraphBlock(
						new TextInline(
							"Duis autem vel eum iriure dolor in hendrerit in vulputate velit esse molestie " +
							"consequat, vel illum dolore eu feugiat nulla facilisis at vero eros et accumsan et " +
							"iusto odio dignissim qui blandit praesent luptatum zzril delenit augue duis dolore te " +
							"feugait nulla facilisi. Lorem ipsum dolor sit amet, consectetuer adipiscing elit, sed " +
							"diam nonummy nibh euismod tincidunt ut laoreet dolore magna aliquam erat volutpat.")))),
			new HeadingBlock(
				new TextInline("Lists"))
			{ Level = HeadingLevel.Level2 },
			new ListBlock(
				new ListItem(
					new TextInline("This is a list")),
				new ListItem(
					new GenericBlock(
						new TextInline("It has multiple items")),
					new ListBlock(
						new ListItem(
							new TextInline("Lists can be nested")),
						new ListItem(
							new GenericBlock(
								new TextInline("The nested lists can have different styles:")),
							new ListBlock(
								new ListItem(
									new TextInline("Ordered")),
								new ListItem(
									new TextInline("Unordered")))
							{ Style = ListStyle.Ordered })))),
			new HeadingBlock(
				new TextInline("Code"))
			{ Level = HeadingLevel.Level2 },
			new ParagraphBlock(
				new TextInline("Code can be presented either "),
				new CodeInline(
					new TextInline("inline")),
				new TextInline(",")),
			new CodeBlock("or as a code\n   block."),
			new HeadingBlock(
				new TextInline("Images"))
			{ Level = HeadingLevel.Level2 },
			new ParagraphBlock(
				new ItalicInline(
					new TextInline("This is a "),
					new HyperlinkInline(
						new TextInline("paragraph "),
						new CodeInline(
							new BoldInline(
								new TextInline("containing "),
								new ImageInline(new Uri("https://upload.wikimedia.org/wikipedia/mediawiki/a/a9/Example.jpg")),
								new TextInline(" an"))))
					{ Target = new Uri("https://www.example.com") },
					new TextInline(" image"))));
		//var navigationService = Ioc.Default.GetRequiredService<INavigationService>();
		//var updateLabel = () => _lbl.Text = navigationService.CurrentRoute.FeedSource?.Name ?? "Unknown feed";
		//navigationService.BackStackChanged += (s, e) => updateLabel();
		//updateLabel();
	}

	private RichText _richText;
}
