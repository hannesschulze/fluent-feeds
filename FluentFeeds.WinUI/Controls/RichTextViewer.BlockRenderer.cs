using System;
using System.Linq;
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
using Windows.Foundation;
using Windows.UI;
using MUXD = Microsoft.UI.Xaml.Documents;

namespace FluentFeeds.WinUI.Controls;

public partial class RichTextViewer
{
	/// <summary>
	/// Cell in a table.
	/// </summary>
	private readonly record struct TableCell(
		StackPanel Content, Size NaturalSize, int RowStart, int RowSpan, int ColumnStart, int ColumnSpan);

	/// <summary>
	/// Renders blocks and appends them to a stack panel.
	/// </summary>
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
			var cells = ComputeTableCells(block, out var rowCount, out var columnCount);
			var columnSizes = ComputeTableColumnSizes(columnCount, cells);

			var grid = 
				new Grid
				{
					HorizontalAlignment = HorizontalAlignment.Left,
					MaxWidth = columnSizes.Sum() + 1,
					BorderThickness = new Thickness(0, 0, right: 1, bottom: 1),
					BorderBrush = new SolidColorBrush(Color.FromArgb(255, 0, 255, 0))
				};

			for (var row = 0; row < rowCount; ++row)
			{
				grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
			}

			foreach (var columnSize in columnSizes)
			{
				grid.ColumnDefinitions.Add(
					new ColumnDefinition { Width = new GridLength(columnSize, GridUnitType.Star) });
			}

			foreach (var cell in cells)
			{
				Grid.SetRow(cell.Content, cell.RowStart);
				Grid.SetRowSpan(cell.Content, cell.RowSpan);
				Grid.SetColumn(cell.Content, cell.ColumnStart);
				Grid.SetColumnSpan(cell.Content, cell.ColumnSpan);
				grid.Children.Add(cell.Content);
			}

			AddElement(grid, 14, 14);
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

		private static List<TableCell> ComputeTableCells(TableBlock block, out int rowCount, out int columnCount)
		{
			var cells = new List<TableCell>();
			var rowOffsets = new List<int>();
			for (var rowIndex = 0; rowIndex < block.Rows.Length; ++rowIndex)
			{
				// The row always matches the one requested.
				var row = block.Rows[rowIndex];
				var columnIndex = 0;
				foreach (var cell in row.Cells)
				{
					if (cell.RowSpan == 0 || cell.ColumnSpan == 0)
						continue;

					while (columnIndex < rowOffsets.Count && rowOffsets[columnIndex] > 0)
					{
						// This column is blocked by a row above the current one -> move to the next one.
						rowOffsets[columnIndex]--;
						columnIndex++;
					}

					var columnStart = columnIndex;
					var columnSpan = 0;
					for (; columnIndex < columnStart + cell.ColumnSpan; ++columnIndex)
					{
						if (columnIndex < rowOffsets.Count && rowOffsets[columnIndex] > 0)
							// The requested column span is blocked by another cell -> shrink the column.
							break;

						// Reserve the requested row span for each column the cell blocks.
						columnSpan++;
						while (rowOffsets.Count <= columnIndex)
							rowOffsets.Add(0);
						rowOffsets[columnIndex] += cell.RowSpan - 1;
					}

					var content =
						new StackPanel
						{
							HorizontalAlignment = HorizontalAlignment.Stretch,
							VerticalAlignment = VerticalAlignment.Stretch,
							Padding = new Thickness(4),
							BorderThickness = new Thickness(left: 1, top: 1, 0, 0),
							BorderBrush = new SolidColorBrush(Color.FromArgb(255, 0, 255, 0))
						};
					if (cell.IsHeader)
						content.Background = new SolidColorBrush(Color.FromArgb(50, 10, 90, 230));
					var renderer = new BlockRenderer(content);
					foreach (var itemBlock in cell.Blocks)
						itemBlock.Accept(renderer);
					content.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));

					cells.Add(new TableCell(
						Content: content,
						NaturalSize: content.DesiredSize,
						RowStart: rowIndex,
						RowSpan: cell.RowSpan,
						ColumnStart: columnStart,
						ColumnSpan: columnSpan));
				}
			}

			rowCount = block.Rows.Length + rowOffsets.Max();
			columnCount = rowOffsets.Count;
			return cells;
		}

		private static double[] ComputeTableColumnSizes(int columnCount, IReadOnlyList<TableCell> cells)
		{
			// Determine the maximum width for every column
			// Pass 1: single-column cells
			var columnSizes = new double[columnCount];
			foreach (var cell in cells)
			{
				if (cell.ColumnSpan == 1)
				{
					var column = cell.ColumnStart;
					columnSizes[column] = Math.Max(columnSizes[column], cell.NaturalSize.Width);
				}
			}

			// Pass 2: multi-column cells -> subtract already allocated widths
			foreach (var cell in cells)
			{
				if (cell.ColumnSpan > 1)
				{
					var remainingWidth = cell.NaturalSize.Width;
					for (var column = cell.ColumnStart; column < cell.ColumnStart + cell.ColumnSpan; ++column)
						remainingWidth -= columnSizes[column];
					var additionalWidth = Math.Max(remainingWidth, 0.0) / cell.ColumnSpan;
					for (var column = cell.ColumnStart; column < cell.ColumnStart + cell.ColumnSpan; ++column)
						columnSizes[column] += additionalWidth;
				}
			}

			return columnSizes;
		}

		private readonly StackPanel _container;
		private RichTextBlock? _lastRichTextBlock;
		private double _trailingMargin = 0;
	}
}
