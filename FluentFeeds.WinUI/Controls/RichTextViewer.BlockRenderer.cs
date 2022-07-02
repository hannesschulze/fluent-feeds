using System;
using System.Collections.Generic;
using FluentFeeds.Shared.RichText.Blocks;
using FluentFeeds.Shared.RichText.Blocks.Heading;
using FluentFeeds.Shared.RichText.Blocks.List;
using FluentFeeds.Shared.RichText.Inlines;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using Windows.UI;
using MUXD = Microsoft.UI.Xaml.Documents;

namespace FluentFeeds.WinUI.Controls;

public partial class RichTextViewer
{
	private sealed class BlockRenderer : IBlockVisitor
	{
		public BlockRenderer(StackPanel container)
		{
			_container = container;
		}

		public void Visit(GenericBlock block)
		{
			var paragraph = CreateParagraph(block.Inlines);
			paragraph.LineHeight = 20;
			paragraph.FontSize = 14;
			AddBlock(paragraph, 0, 0);
		}

		public void Visit(ParagraphBlock block)
		{
			var paragraph = CreateParagraph(block.Inlines);
			paragraph.LineHeight = 20;
			paragraph.FontSize = 14;
			AddBlock(paragraph, 14, 14);
		}

		public void Visit(HeadingBlock block)
		{
			var paragraph = CreateParagraph(block.Inlines);
			switch (block.Level)
			{
				case HeadingLevel.Level1:
					paragraph.LineHeight = 36;
					paragraph.FontSize = 28;
					paragraph.FontWeight = FontWeights.SemiBold;
					break;
				case HeadingLevel.Level2:
					paragraph.LineHeight = 28;
					paragraph.FontSize = 21;
					paragraph.FontWeight = FontWeights.SemiBold;
					break;
				case HeadingLevel.Level3:
					paragraph.LineHeight = 22;
					paragraph.FontSize = 16;
					paragraph.FontWeight = FontWeights.SemiBold;
					break;
				case HeadingLevel.Level4:
					paragraph.LineHeight = 20;
					paragraph.FontSize = 14;
					paragraph.FontWeight = FontWeights.Bold;
					break;
				case HeadingLevel.Level5:
					paragraph.LineHeight = 15;
					paragraph.FontSize = 12;
					paragraph.FontWeight = FontWeights.Bold;
					break;
				case HeadingLevel.Level6:
					paragraph.LineHeight = 12;
					paragraph.FontSize = 10;
					paragraph.FontWeight = FontWeights.Bold;
					break;
			}
			AddBlock(paragraph, 20, 20);
		}

		public void Visit(CodeBlock block)
		{
			AddElement(
				new ScrollViewer
				{
					HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
					HorizontalScrollMode = ScrollMode.Auto,
					VerticalScrollBarVisibility = ScrollBarVisibility.Disabled,
					VerticalScrollMode = ScrollMode.Disabled,
					Background = new SolidColorBrush(Color.FromArgb(255, 0, 80, 90)),
					Content =
						new TextBlock
						{
							FontSize = 14,
							LineHeight = 20,
							Text = block.Code,
							IsTextSelectionEnabled = true,
							Margin = new Thickness(8)
						}
				}, 14, 14);
		}

		public void Visit(HorizontalRuleBlock block)
		{
			AddElement(
				new Rectangle
				{
					HorizontalAlignment = HorizontalAlignment.Stretch,
					Height = 1,
					Fill = new SolidColorBrush(Color.FromArgb(255, 0, 255, 0))
				}, 8, 8);
		}

		public void Visit(ListBlock block)
		{
			var grid = new Grid();
			grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(20) });
			grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
			grid.ColumnSpacing = 16;
			for (var i = 0; i < block.Items.Length; ++i)
			{
				var item = block.Items[i];
				grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

				var bullet =
					new TextBlock
					{
						HorizontalAlignment = HorizontalAlignment.Right,
						FontSize = 14,
						LineHeight = 20,
						Text =
							block.Style switch
							{
								ListStyle.Ordered => $"{i + 1}.",
								ListStyle.Unordered => "•",
								_ => throw new IndexOutOfRangeException()
							}
					};
				Grid.SetRow(bullet, i);
				Grid.SetColumn(bullet, 0);
				grid.Children.Add(bullet);

				var content =
					new StackPanel
					{
						HorizontalAlignment = HorizontalAlignment.Stretch,
						Background = new SolidColorBrush(Color.FromArgb(80, 0, 0, 255))
					};
				var renderer = new BlockRenderer(content);
				foreach (var itemBlock in item.Blocks)
					itemBlock.Accept(renderer);
				Grid.SetRow(content, i);
				Grid.SetColumn(content, 1);
				grid.Children.Add(content);
			}

			AddElement(grid, 14, 14);
		}

		public void Visit(QuoteBlock block)
		{
			var grid = new Grid();
			grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(25) });
			grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
			grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

			var line =
				new Rectangle
				{
					HorizontalAlignment = HorizontalAlignment.Center,
					VerticalAlignment = VerticalAlignment.Stretch,
					Width = 3,
					Fill = new SolidColorBrush(Color.FromArgb(255, 90, 150, 0))
				};
			Grid.SetRow(line, 0);
			Grid.SetColumn(line, 0);
			grid.Children.Add(line);

			var content =
				new StackPanel
				{
					HorizontalAlignment = HorizontalAlignment.Stretch,
					Background = new SolidColorBrush(Color.FromArgb(80, 90, 150, 0))
				};
			var renderer = new BlockRenderer(content);
			foreach (var child in block.Blocks)
				child.Accept(renderer);
			Grid.SetRow(content, 0);
			Grid.SetColumn(content, 1);
			grid.Children.Add(content);

			AddElement(grid, 14, 14);
		}

		public void Visit(TableBlock block)
		{
			// TODO: Render tables
			throw new System.NotImplementedException();
		}

		private void AddElement(FrameworkElement element, double marginLeading, double marginTrailing)
		{
			element.Margin = GetNextMargin(marginLeading, marginTrailing);
			_container.Children.Add(element);
			_lastRichTextBlock = null;
		}

		private void AddBlock(MUXD.Block block, double marginLeading, double marginTrailing)
		{
			if (_lastRichTextBlock == null)
			{
				_lastRichTextBlock =
					new RichTextBlock
					{
						HorizontalAlignment = HorizontalAlignment.Stretch
					};
				_container.Children.Add(_lastRichTextBlock);
			}

			// Blocks already merge the margins themselves, add outer margin to rich text block.
			block.Margin = new Thickness(0, top: marginLeading, 0, bottom: marginTrailing);
			var blockMargin = GetNextMargin(marginLeading, marginTrailing);
			if (_lastRichTextBlock.Blocks.Count == 0)
				_lastRichTextBlock.Margin = blockMargin;
			else
				_lastRichTextBlock.Margin = _lastRichTextBlock.Margin with { Bottom = blockMargin.Bottom };

			_lastRichTextBlock.Blocks.Add(block);
		}

		private MUXD.Paragraph CreateParagraph(IEnumerable<Inline> inlines)
		{
			var paragraph = new MUXD.Paragraph();
			var renderer = new InlineRenderer(paragraph.Inlines, new InlineRenderingContext());
			foreach (var inline in inlines)
				inline.Accept(renderer);
			return paragraph;
		}

		private Thickness GetNextMargin(double marginLeading, double marginTrailing)
		{
			// Remove the already existing trailing margin from the previous block.
			var actualLeading = Math.Max(0.0, marginLeading - _trailingMargin);
			_trailingMargin = marginTrailing;
			return new Thickness(0, top: actualLeading, 0, bottom: marginTrailing);
		}

		private readonly StackPanel _container;
		private RichTextBlock? _lastRichTextBlock;
		private double _trailingMargin = 0;
	}
}
