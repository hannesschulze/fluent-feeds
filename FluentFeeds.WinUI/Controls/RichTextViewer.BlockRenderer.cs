using System;
using System.Linq;
using System.Collections.Generic;
using FluentFeeds.Shared.RichText.Blocks;
using FluentFeeds.Shared.RichText.Blocks.Heading;
using FluentFeeds.Shared.RichText.Blocks.List;
using FluentFeeds.Shared.RichText.Inlines;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Shapes;
using Microsoft.UI.Xaml.Media;
using Windows.Foundation;
using Windows.UI.Text;
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
	/// Current context of a <c>BlockRenderer</c>, used to override various format settings.
	/// </summary>
	private readonly record struct BlockRenderingContext()
	{
		public FontWeight? FontWeightOverride { get; init; } = null;
		public Brush? ForegroundOverride { get; init; } = null;
	}

	/// <summary>
	/// Renders blocks and appends them to a stack panel.
	/// </summary>
	private sealed class BlockRenderer : IBlockVisitor
	{
		public BlockRenderer(RichTextViewer viewer, StackPanel container, in BlockRenderingContext context)
		{
			Viewer = viewer;
			Container = container;
			Context = context;
		}

		public RichTextViewer Viewer { get; }

		public StackPanel Container { get; }

		public BlockRenderingContext Context { get; }

		public void Visit(GenericBlock block)
		{
			var paragraph = CreateParagraph(block.Inlines);
			paragraph.FontFamily = Viewer.BodyFontFamily;
			paragraph.FontWeight = Context.FontWeightOverride ?? Viewer.BodyFontWeight;
			paragraph.FontSize = Viewer.BodyFontSize;
			paragraph.LineHeight = Viewer.BodyLineHeight;
			paragraph.Foreground = Context.ForegroundOverride ?? Viewer.BodyForeground;
			AddBlock(paragraph, Viewer.BodyDefaultMargin);
		}

		public void Visit(ParagraphBlock block)
		{
			var paragraph = CreateParagraph(block.Inlines);
			paragraph.FontFamily = Viewer.BodyFontFamily;
			paragraph.FontWeight = Context.FontWeightOverride ?? Viewer.BodyFontWeight;
			paragraph.FontSize = Viewer.BodyFontSize;
			paragraph.LineHeight = Viewer.BodyLineHeight;
			paragraph.Foreground = Context.ForegroundOverride ?? Viewer.BodyForeground;
			AddBlock(paragraph, Viewer.BodyParagraphMargin);
		}

		public void Visit(HeadingBlock block)
		{
			var paragraph = CreateParagraph(block.Inlines);
			switch (block.Level)
			{
				case HeadingLevel.Level1:
					paragraph.FontFamily = Viewer.Heading1FontFamily;
					paragraph.FontWeight = Context.FontWeightOverride ?? Viewer.Heading1FontWeight;
					paragraph.FontSize = Viewer.Heading1FontSize;
					paragraph.LineHeight = Viewer.Heading1LineHeight;
					paragraph.Foreground = Context.ForegroundOverride ?? Viewer.Heading1Foreground;
					AddBlock(paragraph, Viewer.Heading1Margin);
					break;
				case HeadingLevel.Level2:
					paragraph.FontFamily = Viewer.Heading2FontFamily;
					paragraph.FontWeight = Context.FontWeightOverride ?? Viewer.Heading2FontWeight;
					paragraph.FontSize = Viewer.Heading2FontSize;
					paragraph.LineHeight = Viewer.Heading2LineHeight;
					paragraph.Foreground = Context.ForegroundOverride ?? Viewer.Heading2Foreground;
					AddBlock(paragraph, Viewer.Heading2Margin);
					break;
				case HeadingLevel.Level3:
					paragraph.FontFamily = Viewer.Heading3FontFamily;
					paragraph.FontWeight = Context.FontWeightOverride ?? Viewer.Heading3FontWeight;
					paragraph.FontSize = Viewer.Heading3FontSize;
					paragraph.LineHeight = Viewer.Heading3LineHeight;
					paragraph.Foreground = Context.ForegroundOverride ?? Viewer.Heading3Foreground;
					AddBlock(paragraph, Viewer.Heading3Margin);
					break;
				case HeadingLevel.Level4:
					paragraph.FontFamily = Viewer.Heading4FontFamily;
					paragraph.FontWeight = Context.FontWeightOverride ?? Viewer.Heading4FontWeight;
					paragraph.FontSize = Viewer.Heading4FontSize;
					paragraph.LineHeight = Viewer.Heading4LineHeight;
					paragraph.Foreground = Context.ForegroundOverride ?? Viewer.Heading4Foreground;
					AddBlock(paragraph, Viewer.Heading4Margin);
					break;
				case HeadingLevel.Level5:
					paragraph.FontFamily = Viewer.Heading5FontFamily;
					paragraph.FontWeight = Context.FontWeightOverride ?? Viewer.Heading5FontWeight;
					paragraph.FontSize = Viewer.Heading5FontSize;
					paragraph.LineHeight = Viewer.Heading5LineHeight;
					paragraph.Foreground = Context.ForegroundOverride ?? Viewer.Heading5Foreground;
					AddBlock(paragraph, Viewer.Heading5Margin);
					break;
				case HeadingLevel.Level6:
					paragraph.FontFamily = Viewer.Heading6FontFamily;
					paragraph.FontWeight = Context.FontWeightOverride ?? Viewer.Heading6FontWeight;
					paragraph.FontSize = Viewer.Heading6FontSize;
					paragraph.LineHeight = Viewer.Heading6LineHeight;
					paragraph.Foreground = Context.ForegroundOverride ?? Viewer.Heading6Foreground;
					AddBlock(paragraph, Viewer.Heading6Margin);
					break;
			}
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
					Background = Viewer.CodeBackground,
					CornerRadius = Viewer.CodeCornerRadius,
					BorderBrush = Viewer.CodeBorderBrush,
					BorderThickness = Viewer.CodeBorderThickness,
					Content =
						new TextBlock
						{
							FontFamily = Viewer.CodeFontFamily,
							FontWeight = Viewer.CodeFontWeight,
							FontSize = Viewer.CodeFontSize,
							LineHeight = Viewer.CodeLineHeight,
							Foreground = Viewer.CodeForeground,
							Margin = Viewer.CodePadding,
							Text = block.Code,
							IsTextSelectionEnabled = true
						}
				}, Viewer.CodeMargin);
		}

		public void Visit(HorizontalRuleBlock block)
		{
			AddElement(
				new Rectangle
				{
					HorizontalAlignment = HorizontalAlignment.Stretch,
					Height = Viewer.DividerThickness,
					Fill = Viewer.DividerBrush
				}, Viewer.DividerMargin);
		}

		public void Visit(ListBlock block)
		{
			var grid = new Grid();
			grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(Viewer.ListBulletIndent) });
			grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
			grid.ColumnSpacing = Viewer.ListBulletSpacing;
			for (var i = 0; i < block.Items.Length; ++i)
			{
				var item = block.Items[i];
				grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

				var bullet =
					new TextBlock
					{
						HorizontalAlignment = HorizontalAlignment.Right,
						FontFamily = Viewer.BodyFontFamily,
						FontWeight = Viewer.BodyFontWeight,
						FontSize = Viewer.BodyFontSize,
						LineHeight = Viewer.BodyLineHeight,
						Foreground = Context.ForegroundOverride ?? Viewer.BodyForeground,
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

				var content = new StackPanel { HorizontalAlignment = HorizontalAlignment.Stretch };
				var renderer = new BlockRenderer(Viewer, content, Context);
				foreach (var itemBlock in item.Blocks)
					itemBlock.Accept(renderer);
				Grid.SetRow(content, i);
				Grid.SetColumn(content, 1);
				grid.Children.Add(content);
			}

			AddElement(grid, Viewer.ListMargin);
		}

		public void Visit(QuoteBlock block)
		{
			var grid = new Grid();
			grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(Viewer.QuoteIndent) });
			grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
			grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

			var line =
				new Rectangle
				{
					HorizontalAlignment = HorizontalAlignment.Center,
					VerticalAlignment = VerticalAlignment.Stretch,
					Width = Viewer.QuoteIndicatorThickness,
					Fill = Viewer.QuoteIndicatorBrush,
					RadiusX = Viewer.QuoteIndicatorThickness * 0.5,
					RadiusY = Viewer.QuoteIndicatorThickness * 0.5
				};
			Grid.SetRow(line, 0);
			Grid.SetColumn(line, 0);
			grid.Children.Add(line);

			var content = new StackPanel { HorizontalAlignment = HorizontalAlignment.Stretch };
			var renderer = new BlockRenderer(Viewer, content, Context);
			foreach (var child in block.Blocks)
				child.Accept(renderer);
			Grid.SetRow(content, 0);
			Grid.SetColumn(content, 1);
			grid.Children.Add(content);

			AddElement(grid, Viewer.QuoteMargin);
		}

		public void Visit(TableBlock block)
		{
			var cells = ComputeTableCells(block, out var rowCount, out var columnCount);
			var columnSizes = ComputeTableColumnSizes(columnCount, cells);

			var grid = 
				new Grid
				{
					HorizontalAlignment = HorizontalAlignment.Left,
					MaxWidth = columnSizes.Sum() + Viewer.DividerThickness,
					BorderThickness = 
						new Thickness(0, 0, right: Viewer.DividerThickness, bottom: Viewer.DividerThickness),
					BorderBrush = Viewer.DividerBrush
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

			AddElement(grid, Viewer.TableMargin);
		}

		private void AddElement(FrameworkElement element, double margin)
		{
			element.Margin = GetNextMargin(margin);
			Container.Children.Add(element);
			_lastRichTextBlock = null;
		}

		private void AddBlock(MUXD.Block block, double margin)
		{
			if (_lastRichTextBlock == null)
			{
				_lastRichTextBlock =
					new RichTextBlock
					{
						HorizontalAlignment = HorizontalAlignment.Stretch
					};
				Container.Children.Add(_lastRichTextBlock);
			}

			// Blocks already merge the margins themselves, add outer margin to rich text block.
			block.Margin = new Thickness(0, top: margin, 0, bottom: margin);
			var blockMargin = GetNextMargin(margin);
			if (_lastRichTextBlock.Blocks.Count == 0)
				_lastRichTextBlock.Margin = blockMargin;
			else
				_lastRichTextBlock.Margin = _lastRichTextBlock.Margin with { Bottom = blockMargin.Bottom };

			_lastRichTextBlock.Blocks.Add(block);
		}

		private MUXD.Paragraph CreateParagraph(IEnumerable<Inline> inlines)
		{
			var paragraph = new MUXD.Paragraph();
			var renderer = new InlineRenderer(Viewer, paragraph.Inlines, new InlineRenderingContext());
			foreach (var inline in inlines)
				inline.Accept(renderer);
			return paragraph;
		}

		private Thickness GetNextMargin(double margin)
		{
			// Remove the already existing trailing margin from the previous block.
			var actualLeading = Math.Max(0.0, margin - _trailingMargin);
			_trailingMargin = margin;
			return new Thickness(0, top: actualLeading, 0, bottom: margin);
		}

		private List<TableCell> ComputeTableCells(TableBlock block, out int rowCount, out int columnCount)
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
							Padding = Viewer.TablePadding,
							BorderThickness = 
								new Thickness(left: Viewer.DividerThickness, top: Viewer.DividerThickness, 0, 0),
							BorderBrush = Viewer.DividerBrush
						};
					var contentContext = Context;
					if (cell.IsHeader)
					{
						contentContext =
							contentContext with
							{
								FontWeightOverride = Viewer.TableHeaderFontWeight,
								ForegroundOverride = Viewer.TableHeaderForeground,
							};
						content.Background = Viewer.TableHeaderBackground;
					}
					var renderer = new BlockRenderer(Viewer, content, contentContext);
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

		private RichTextBlock? _lastRichTextBlock;
		private double _trailingMargin = 0;
	}
}
