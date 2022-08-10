using FluentFeeds.Documents;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Markup;
using Microsoft.UI.Xaml.Media;
using Windows.UI.Text;

namespace FluentFeeds.App.WinUI.Controls;

/// <summary>
/// A control which displays a generic <see cref="RichText"/> object.
/// </summary>
[ContentProperty(Name = "RichText")]
public sealed partial class RichTextViewer : Control
{
	public static readonly DependencyProperty RichTextProperty = DependencyProperty.Register(
		nameof(RichText), typeof(RichText), typeof(RichTextViewer), new PropertyMetadata(null, HandlePropertyChanged));

	public static readonly DependencyProperty CodeFontFamilyProperty = DependencyProperty.Register(
		nameof(CodeFontFamily), typeof(FontFamily), typeof(RichTextViewer),
		new PropertyMetadata(null, HandlePropertyChanged));
	public static readonly DependencyProperty CodeFontWeightProperty = DependencyProperty.Register(
		nameof(CodeFontWeight), typeof(FontWeight), typeof(RichTextViewer),
		new PropertyMetadata(FontWeights.Normal, HandlePropertyChanged));
	public static readonly DependencyProperty CodeFontSizeProperty = DependencyProperty.Register(
		nameof(CodeFontSize), typeof(double), typeof(RichTextViewer),
		new PropertyMetadata(0.0, HandlePropertyChanged));
	public static readonly DependencyProperty CodeLineHeightProperty = DependencyProperty.Register(
		nameof(CodeLineHeight), typeof(double), typeof(RichTextViewer),
		new PropertyMetadata(0.0, HandlePropertyChanged));
	public static readonly DependencyProperty CodeBackgroundProperty = DependencyProperty.Register(
		nameof(CodeBackground), typeof(Brush), typeof(RichTextViewer),
		new PropertyMetadata(null, HandlePropertyChanged));
	public static readonly DependencyProperty CodeForegroundProperty = DependencyProperty.Register(
		nameof(CodeForeground), typeof(Brush), typeof(RichTextViewer),
		new PropertyMetadata(null, HandlePropertyChanged));
	public static readonly DependencyProperty CodeBorderBrushProperty = DependencyProperty.Register(
		nameof(CodeBorderBrush), typeof(Brush), typeof(RichTextViewer),
		new PropertyMetadata(null, HandlePropertyChanged));
	public static readonly DependencyProperty CodeBorderThicknessProperty = DependencyProperty.Register(
		nameof(CodeBorderThickness), typeof(Thickness), typeof(RichTextViewer),
		new PropertyMetadata(new Thickness(), HandlePropertyChanged));
	public static readonly DependencyProperty CodePaddingProperty = DependencyProperty.Register(
		nameof(CodePadding), typeof(Thickness), typeof(RichTextViewer),
		new PropertyMetadata(new Thickness(), HandlePropertyChanged));
	public static readonly DependencyProperty CodeMarginProperty = DependencyProperty.Register(
		nameof(CodeMargin), typeof(double), typeof(RichTextViewer),
		new PropertyMetadata(0.0, HandlePropertyChanged));
	public static readonly DependencyProperty CodeCornerRadiusProperty = DependencyProperty.Register(
		nameof(CodeCornerRadius), typeof(CornerRadius), typeof(RichTextViewer),
		new PropertyMetadata(new CornerRadius(), HandlePropertyChanged));

	public static readonly DependencyProperty HyperlinkForegroundProperty = DependencyProperty.Register(
		nameof(HyperlinkForeground), typeof(Brush), typeof(RichTextViewer),
		new PropertyMetadata(null, HandlePropertyChanged));

	public static readonly DependencyProperty BodyFontFamilyProperty = DependencyProperty.Register(
		nameof(BodyFontFamily), typeof(FontFamily), typeof(RichTextViewer),
		new PropertyMetadata(null, HandlePropertyChanged));
	public static readonly DependencyProperty BodyFontWeightProperty = DependencyProperty.Register(
		nameof(BodyFontWeight), typeof(FontWeight), typeof(RichTextViewer),
		new PropertyMetadata(FontWeights.Normal, HandlePropertyChanged));
	public static readonly DependencyProperty BodyFontSizeProperty = DependencyProperty.Register(
		nameof(BodyFontSize), typeof(double), typeof(RichTextViewer),
		new PropertyMetadata(0.0, HandlePropertyChanged));
	public static readonly DependencyProperty BodyLineHeightProperty = DependencyProperty.Register(
		nameof(BodyLineHeight), typeof(double), typeof(RichTextViewer),
		new PropertyMetadata(0.0, HandlePropertyChanged));
	public static readonly DependencyProperty BodyForegroundProperty = DependencyProperty.Register(
		nameof(BodyForeground), typeof(Brush), typeof(RichTextViewer),
		new PropertyMetadata(null, HandlePropertyChanged));
	public static readonly DependencyProperty BodyDefaultMarginProperty = DependencyProperty.Register(
		nameof(BodyDefaultMargin), typeof(double), typeof(RichTextViewer),
		new PropertyMetadata(0.0, HandlePropertyChanged));
	public static readonly DependencyProperty BodyParagraphMarginProperty = DependencyProperty.Register(
		nameof(BodyParagraphMargin), typeof(double), typeof(RichTextViewer),
		new PropertyMetadata(0.0, HandlePropertyChanged));

	public static readonly DependencyProperty Heading1FontFamilyProperty = DependencyProperty.Register(
		nameof(Heading1FontFamily), typeof(FontFamily), typeof(RichTextViewer),
		new PropertyMetadata(null, HandlePropertyChanged));
	public static readonly DependencyProperty Heading1FontWeightProperty = DependencyProperty.Register(
		nameof(Heading1FontWeight), typeof(FontWeight), typeof(RichTextViewer),
		new PropertyMetadata(FontWeights.Normal, HandlePropertyChanged));
	public static readonly DependencyProperty Heading1FontSizeProperty = DependencyProperty.Register(
		nameof(Heading1FontSize), typeof(double), typeof(RichTextViewer),
		new PropertyMetadata(0.0, HandlePropertyChanged));
	public static readonly DependencyProperty Heading1LineHeightProperty = DependencyProperty.Register(
		nameof(Heading1LineHeight), typeof(double), typeof(RichTextViewer),
		new PropertyMetadata(0.0, HandlePropertyChanged));
	public static readonly DependencyProperty Heading1ForegroundProperty = DependencyProperty.Register(
		nameof(Heading1Foreground), typeof(Brush), typeof(RichTextViewer),
		new PropertyMetadata(null, HandlePropertyChanged));
	public static readonly DependencyProperty Heading1MarginProperty = DependencyProperty.Register(
		nameof(Heading1Margin), typeof(double), typeof(RichTextViewer),
		new PropertyMetadata(0.0, HandlePropertyChanged));

	public static readonly DependencyProperty Heading2FontFamilyProperty = DependencyProperty.Register(
		nameof(Heading2FontFamily), typeof(FontFamily), typeof(RichTextViewer),
		new PropertyMetadata(null, HandlePropertyChanged));
	public static readonly DependencyProperty Heading2FontWeightProperty = DependencyProperty.Register(
		nameof(Heading2FontWeight), typeof(FontWeight), typeof(RichTextViewer),
		new PropertyMetadata(FontWeights.Normal, HandlePropertyChanged));
	public static readonly DependencyProperty Heading2FontSizeProperty = DependencyProperty.Register(
		nameof(Heading2FontSize), typeof(double), typeof(RichTextViewer),
		new PropertyMetadata(0.0, HandlePropertyChanged));
	public static readonly DependencyProperty Heading2LineHeightProperty = DependencyProperty.Register(
		nameof(Heading2LineHeight), typeof(double), typeof(RichTextViewer),
		new PropertyMetadata(0.0, HandlePropertyChanged));
	public static readonly DependencyProperty Heading2ForegroundProperty = DependencyProperty.Register(
		nameof(Heading2Foreground), typeof(Brush), typeof(RichTextViewer),
		new PropertyMetadata(null, HandlePropertyChanged));
	public static readonly DependencyProperty Heading2MarginProperty = DependencyProperty.Register(
		nameof(Heading2Margin), typeof(double), typeof(RichTextViewer),
		new PropertyMetadata(0.0, HandlePropertyChanged));

	public static readonly DependencyProperty Heading3FontFamilyProperty = DependencyProperty.Register(
		nameof(Heading3FontFamily), typeof(FontFamily), typeof(RichTextViewer),
		new PropertyMetadata(null, HandlePropertyChanged));
	public static readonly DependencyProperty Heading3FontWeightProperty = DependencyProperty.Register(
		nameof(Heading3FontWeight), typeof(FontWeight), typeof(RichTextViewer),
		new PropertyMetadata(FontWeights.Normal, HandlePropertyChanged));
	public static readonly DependencyProperty Heading3FontSizeProperty = DependencyProperty.Register(
		nameof(Heading3FontSize), typeof(double), typeof(RichTextViewer),
		new PropertyMetadata(0.0, HandlePropertyChanged));
	public static readonly DependencyProperty Heading3LineHeightProperty = DependencyProperty.Register(
		nameof(Heading3LineHeight), typeof(double), typeof(RichTextViewer),
		new PropertyMetadata(0.0, HandlePropertyChanged));
	public static readonly DependencyProperty Heading3ForegroundProperty = DependencyProperty.Register(
		nameof(Heading3Foreground), typeof(Brush), typeof(RichTextViewer),
		new PropertyMetadata(null, HandlePropertyChanged));
	public static readonly DependencyProperty Heading3MarginProperty = DependencyProperty.Register(
		nameof(Heading3Margin), typeof(double), typeof(RichTextViewer),
		new PropertyMetadata(0.0, HandlePropertyChanged));

	public static readonly DependencyProperty Heading4FontFamilyProperty = DependencyProperty.Register(
		nameof(Heading4FontFamily), typeof(FontFamily), typeof(RichTextViewer),
		new PropertyMetadata(null, HandlePropertyChanged));
	public static readonly DependencyProperty Heading4FontWeightProperty = DependencyProperty.Register(
		nameof(Heading4FontWeight), typeof(FontWeight), typeof(RichTextViewer),
		new PropertyMetadata(FontWeights.Normal, HandlePropertyChanged));
	public static readonly DependencyProperty Heading4FontSizeProperty = DependencyProperty.Register(
		nameof(Heading4FontSize), typeof(double), typeof(RichTextViewer),
		new PropertyMetadata(0.0, HandlePropertyChanged));
	public static readonly DependencyProperty Heading4LineHeightProperty = DependencyProperty.Register(
		nameof(Heading4LineHeight), typeof(double), typeof(RichTextViewer),
		new PropertyMetadata(0.0, HandlePropertyChanged));
	public static readonly DependencyProperty Heading4ForegroundProperty = DependencyProperty.Register(
		nameof(Heading4Foreground), typeof(Brush), typeof(RichTextViewer),
		new PropertyMetadata(null, HandlePropertyChanged));
	public static readonly DependencyProperty Heading4MarginProperty = DependencyProperty.Register(
		nameof(Heading4Margin), typeof(double), typeof(RichTextViewer),
		new PropertyMetadata(0.0, HandlePropertyChanged));

	public static readonly DependencyProperty Heading5FontFamilyProperty = DependencyProperty.Register(
		nameof(Heading5FontFamily), typeof(FontFamily), typeof(RichTextViewer),
		new PropertyMetadata(null, HandlePropertyChanged));
	public static readonly DependencyProperty Heading5FontWeightProperty = DependencyProperty.Register(
		nameof(Heading5FontWeight), typeof(FontWeight), typeof(RichTextViewer),
		new PropertyMetadata(FontWeights.Normal, HandlePropertyChanged));
	public static readonly DependencyProperty Heading5FontSizeProperty = DependencyProperty.Register(
		nameof(Heading5FontSize), typeof(double), typeof(RichTextViewer),
		new PropertyMetadata(0.0, HandlePropertyChanged));
	public static readonly DependencyProperty Heading5LineHeightProperty = DependencyProperty.Register(
		nameof(Heading5LineHeight), typeof(double), typeof(RichTextViewer),
		new PropertyMetadata(0.0, HandlePropertyChanged));
	public static readonly DependencyProperty Heading5ForegroundProperty = DependencyProperty.Register(
		nameof(Heading5Foreground), typeof(Brush), typeof(RichTextViewer),
		new PropertyMetadata(null, HandlePropertyChanged));
	public static readonly DependencyProperty Heading5MarginProperty = DependencyProperty.Register(
		nameof(Heading5Margin), typeof(double), typeof(RichTextViewer),
		new PropertyMetadata(0.0, HandlePropertyChanged));

	public static readonly DependencyProperty Heading6FontFamilyProperty = DependencyProperty.Register(
		nameof(Heading6FontFamily), typeof(FontFamily), typeof(RichTextViewer),
		new PropertyMetadata(null, HandlePropertyChanged));
	public static readonly DependencyProperty Heading6FontWeightProperty = DependencyProperty.Register(
		nameof(Heading6FontWeight), typeof(FontWeight), typeof(RichTextViewer),
		new PropertyMetadata(FontWeights.Normal, HandlePropertyChanged));
	public static readonly DependencyProperty Heading6FontSizeProperty = DependencyProperty.Register(
		nameof(Heading6FontSize), typeof(double), typeof(RichTextViewer),
		new PropertyMetadata(0.0, HandlePropertyChanged));
	public static readonly DependencyProperty Heading6LineHeightProperty = DependencyProperty.Register(
		nameof(Heading6LineHeight), typeof(double), typeof(RichTextViewer),
		new PropertyMetadata(0.0, HandlePropertyChanged));
	public static readonly DependencyProperty Heading6ForegroundProperty = DependencyProperty.Register(
		nameof(Heading6Foreground), typeof(Brush), typeof(RichTextViewer),
		new PropertyMetadata(null, HandlePropertyChanged));
	public static readonly DependencyProperty Heading6MarginProperty = DependencyProperty.Register(
		nameof(Heading6Margin), typeof(double), typeof(RichTextViewer),
		new PropertyMetadata(0.0, HandlePropertyChanged));

	public static readonly DependencyProperty DividerThicknessProperty = DependencyProperty.Register(
		nameof(DividerThickness), typeof(double), typeof(RichTextViewer),
		new PropertyMetadata(0.0, HandlePropertyChanged));
	public static readonly DependencyProperty DividerBrushProperty = DependencyProperty.Register(
		nameof(DividerBrush), typeof(Brush), typeof(RichTextViewer),
		new PropertyMetadata(null, HandlePropertyChanged));
	public static readonly DependencyProperty DividerMarginProperty = DependencyProperty.Register(
		nameof(DividerMargin), typeof(double), typeof(RichTextViewer),
		new PropertyMetadata(0.0, HandlePropertyChanged));

	public static readonly DependencyProperty ListBulletIndentProperty = DependencyProperty.Register(
		nameof(ListBulletIndent), typeof(double), typeof(RichTextViewer),
		new PropertyMetadata(0.0, HandlePropertyChanged));
	public static readonly DependencyProperty ListBulletSpacingProperty = DependencyProperty.Register(
		nameof(ListBulletSpacing), typeof(double), typeof(RichTextViewer),
		new PropertyMetadata(0.0, HandlePropertyChanged));
	public static readonly DependencyProperty ListMarginProperty = DependencyProperty.Register(
		nameof(ListMargin), typeof(double), typeof(RichTextViewer),
		new PropertyMetadata(0.0, HandlePropertyChanged));

	public static readonly DependencyProperty QuoteIndentProperty = DependencyProperty.Register(
		nameof(QuoteIndent), typeof(double), typeof(RichTextViewer),
		new PropertyMetadata(0.0, HandlePropertyChanged));
	public static readonly DependencyProperty QuoteIndicatorThicknessProperty = DependencyProperty.Register(
		nameof(QuoteIndicatorThickness), typeof(double), typeof(RichTextViewer),
		new PropertyMetadata(0.0, HandlePropertyChanged));
	public static readonly DependencyProperty QuoteIndicatorBrushProperty = DependencyProperty.Register(
		nameof(QuoteIndicatorBrush), typeof(Brush), typeof(RichTextViewer),
		new PropertyMetadata(null, HandlePropertyChanged));
	public static readonly DependencyProperty QuoteMarginProperty = DependencyProperty.Register(
		nameof(QuoteMargin), typeof(double), typeof(RichTextViewer),
		new PropertyMetadata(0.0, HandlePropertyChanged));

	public static readonly DependencyProperty TableHeaderFontWeightProperty = DependencyProperty.Register(
		nameof(TableHeaderFontWeight), typeof(FontWeight), typeof(RichTextViewer),
		new PropertyMetadata(FontWeights.Normal, HandlePropertyChanged));
	public static readonly DependencyProperty TableHeaderBackgroundProperty = DependencyProperty.Register(
		nameof(TableHeaderBackground), typeof(Brush), typeof(RichTextViewer),
		new PropertyMetadata(null, HandlePropertyChanged));
	public static readonly DependencyProperty TableHeaderForegroundProperty = DependencyProperty.Register(
		nameof(TableHeaderForeground), typeof(Brush), typeof(RichTextViewer),
		new PropertyMetadata(null, HandlePropertyChanged));
	public static readonly DependencyProperty TablePaddingProperty = DependencyProperty.Register(
		nameof(TablePadding), typeof(Thickness), typeof(RichTextViewer),
		new PropertyMetadata(new Thickness(), HandlePropertyChanged));
	public static readonly DependencyProperty TableMarginProperty = DependencyProperty.Register(
		nameof(TableMargin), typeof(double), typeof(RichTextViewer),
		new PropertyMetadata(0.0, HandlePropertyChanged));

	public RichTextViewer()
	{
		DefaultStyleKey = typeof(RichTextViewer);
	}

	/// <summary>
	/// The text content displayed in this control.
	/// </summary>
	public RichText? RichText
	{
		get => (RichText) GetValue(RichTextProperty);
		set => SetValue(RichTextProperty, value);
	}

	/// <summary>
	/// Font family used in code blocks.
	/// </summary>
	public FontFamily? CodeFontFamily
	{
		get => (FontFamily) GetValue(CodeFontFamilyProperty);
		set => SetValue(CodeFontFamilyProperty, value);
	}

	/// <summary>
	/// Font weight used in code blocks.
	/// </summary>
	public FontWeight CodeFontWeight
	{
		get => (FontWeight)GetValue(CodeFontWeightProperty);
		set => SetValue(CodeFontWeightProperty, value);
	}

	/// <summary>
	/// Font size used in code blocks.
	/// </summary>
	public double CodeFontSize
	{
		get => (double) GetValue(CodeFontSizeProperty);
		set => SetValue(CodeFontSizeProperty, value);
	}

	/// <summary>
	/// Line height used in code blocks.
	/// </summary>
	public double CodeLineHeight
	{
		get => (double) GetValue(CodeLineHeightProperty);
		set => SetValue(CodeLineHeightProperty, value);
	}

	/// <summary>
	/// Background rendered behind a code block.
	/// </summary>
	public Brush? CodeBackground
	{
		get => (Brush) GetValue(CodeBackgroundProperty);
		set => SetValue(CodeBackgroundProperty, value);
	}

	/// <summary>
	/// Foreground used in code blocks.
	/// </summary>
	public Brush? CodeForeground
	{
		get => (Brush)GetValue(CodeForegroundProperty);
		set => SetValue(CodeForegroundProperty, value);
	}

	/// <summary>
	/// Brush used for borders rendered around a code block.
	/// </summary>
	public Brush? CodeBorderBrush
	{
		get => (Brush) GetValue(CodeBorderBrushProperty);
		set => SetValue(CodeBorderBrushProperty, value);
	}

	/// <summary>
	/// Thickness used for borders rendered around a code block.
	/// </summary>
	public Thickness CodeBorderThickness
	{
		get => (Thickness) GetValue(CodeBorderThicknessProperty);
		set => SetValue(CodeBorderThicknessProperty, value);
	}

	/// <summary>
	/// Insets in a code block.
	/// </summary>
	public Thickness CodePadding
	{
		get => (Thickness) GetValue(CodePaddingProperty);
		set => SetValue(CodePaddingProperty, value);
	}

	/// <summary>
	/// Margin displayed around code blocks.
	/// </summary>
	public double CodeMargin
	{
		get => (double) GetValue(CodeMarginProperty);
		set => SetValue(CodeMarginProperty, value);
	}

	/// <summary>
	/// Code block corner radii.
	/// </summary>
	public CornerRadius CodeCornerRadius
	{
		get => (CornerRadius) GetValue(CodeCornerRadiusProperty);
		set => SetValue(CodeCornerRadiusProperty, value);
	}

	/// <summary>
	/// Foreground used for hyperlinks.
	/// </summary>
	public Brush? HyperlinkForeground
	{
		get => (Brush) GetValue(HyperlinkForegroundProperty);
		set => SetValue(HyperlinkForegroundProperty, value);
	}

	/// <summary>
	/// Font family used in paragraphs or generic blocks.
	/// </summary>
	public FontFamily? BodyFontFamily
	{
		get => (FontFamily) GetValue(BodyFontFamilyProperty);
		set => SetValue(BodyFontFamilyProperty, value);
	}

	/// <summary>
	/// Base font weight used in paragraphs or generic blocks.
	/// </summary>
	public FontWeight BodyFontWeight
	{
		get => (FontWeight) GetValue(BodyFontWeightProperty);
		set => SetValue(BodyFontWeightProperty, value);
	}

	/// <summary>
	/// Font size used in paragraphs or generic blocks.
	/// </summary>
	public double BodyFontSize
	{
		get => (double) GetValue(BodyFontSizeProperty);
		set => SetValue(BodyFontSizeProperty, value);
	}

	/// <summary>
	/// Line height used in paragraphs or generic blocks.
	/// </summary>
	public double BodyLineHeight
	{
		get => (double) GetValue(BodyLineHeightProperty);
		set => SetValue(BodyLineHeightProperty, value);
	}

	/// <summary>
	/// The brush used to render the foreground in paragraphs or generic blocks.
	/// </summary>
	public Brush? BodyForeground
	{
		get => (Brush) GetValue(BodyForegroundProperty);
		set => SetValue(BodyForegroundProperty, value);
	}

	/// <summary>
	/// Margin displayed around a generic block.
	/// </summary>
	public double BodyDefaultMargin
	{
		get => (double) GetValue(BodyDefaultMarginProperty);
		set => SetValue(BodyDefaultMarginProperty, value);
	}

	/// <summary>
	/// Margin displayed around a paragraph block.
	/// </summary>
	public double BodyParagraphMargin
	{
		get => (double) GetValue(BodyParagraphMarginProperty);
		set => SetValue(BodyParagraphMarginProperty, value);
	}

	/// <summary>
	/// Font family used in level 1 heading blocks.
	/// </summary>
	public FontFamily? Heading1FontFamily
	{
		get => (FontFamily) GetValue(Heading1FontFamilyProperty);
		set => SetValue(Heading1FontFamilyProperty, value);
	}

	/// <summary>
	/// Base font weight used in level 1 heading blocks.
	/// </summary>
	public FontWeight Heading1FontWeight
	{
		get => (FontWeight) GetValue(Heading1FontWeightProperty);
		set => SetValue(Heading1FontWeightProperty, value);
	}

	/// <summary>
	/// Font size used in level 1 heading blocks.
	/// </summary>
	public double Heading1FontSize
	{
		get => (double) GetValue(Heading1FontSizeProperty);
		set => SetValue(Heading1FontSizeProperty, value);
	}

	/// <summary>
	/// Line height used in level 1 heading blocks.
	/// </summary>
	public double Heading1LineHeight
	{
		get => (double) GetValue(Heading1LineHeightProperty);
		set => SetValue(Heading1LineHeightProperty, value);
	}

	/// <summary>
	/// The brush used to render the foreground in level 1 heading blocks.
	/// </summary>
	public Brush? Heading1Foreground
	{
		get => (Brush) GetValue(Heading1ForegroundProperty);
		set => SetValue(Heading1ForegroundProperty, value);
	}

	/// <summary>
	/// Margin displayed around a level 1 heading block.
	/// </summary>
	public double Heading1Margin
	{
		get => (double) GetValue(Heading1MarginProperty);
		set => SetValue(Heading1MarginProperty, value);
	}

	/// <summary>
	/// Font family used in level 2 heading blocks.
	/// </summary>
	public FontFamily? Heading2FontFamily
	{
		get => (FontFamily) GetValue(Heading2FontFamilyProperty);
		set => SetValue(Heading2FontFamilyProperty, value);
	}

	/// <summary>
	/// Base font weight used in level 2 heading blocks.
	/// </summary>
	public FontWeight Heading2FontWeight
	{
		get => (FontWeight) GetValue(Heading2FontWeightProperty);
		set => SetValue(Heading2FontWeightProperty, value);
	}

	/// <summary>
	/// Font size used in level 2 heading blocks.
	/// </summary>
	public double Heading2FontSize
	{
		get => (double) GetValue(Heading2FontSizeProperty);
		set => SetValue(Heading2FontSizeProperty, value);
	}

	/// <summary>
	/// Line height used in level 2 heading blocks.
	/// </summary>
	public double Heading2LineHeight
	{
		get => (double) GetValue(Heading2LineHeightProperty);
		set => SetValue(Heading2LineHeightProperty, value);
	}

	/// <summary>
	/// The brush used to render the foreground in level 2 heading blocks.
	/// </summary>
	public Brush? Heading2Foreground
	{
		get => (Brush) GetValue(Heading2ForegroundProperty);
		set => SetValue(Heading2ForegroundProperty, value);
	}

	/// <summary>
	/// Margin displayed around a level 2 heading block.
	/// </summary>
	public double Heading2Margin
	{
		get => (double) GetValue(Heading2MarginProperty);
		set => SetValue(Heading2MarginProperty, value);
	}

	/// <summary>
	/// Font family used in level 3 heading blocks.
	/// </summary>
	public FontFamily? Heading3FontFamily
	{
		get => (FontFamily) GetValue(Heading3FontFamilyProperty);
		set => SetValue(Heading3FontFamilyProperty, value);
	}

	/// <summary>
	/// Base font weight used in level 3 heading blocks.
	/// </summary>
	public FontWeight Heading3FontWeight
	{
		get => (FontWeight) GetValue(Heading3FontWeightProperty);
		set => SetValue(Heading3FontWeightProperty, value);
	}

	/// <summary>
	/// Font size used in level 3 heading blocks.
	/// </summary>
	public double Heading3FontSize
	{
		get => (double) GetValue(Heading3FontSizeProperty);
		set => SetValue(Heading3FontSizeProperty, value);
	}

	/// <summary>
	/// Line height used in level 3 heading blocks.
	/// </summary>
	public double Heading3LineHeight
	{
		get => (double) GetValue(Heading3LineHeightProperty);
		set => SetValue(Heading3LineHeightProperty, value);
	}

	/// <summary>
	/// The brush used to render the foreground in level 3 heading blocks.
	/// </summary>
	public Brush? Heading3Foreground
	{
		get => (Brush) GetValue(Heading3ForegroundProperty);
		set => SetValue(Heading3ForegroundProperty, value);
	}

	/// <summary>
	/// Margin displayed around a level 3 heading block.
	/// </summary>
	public double Heading3Margin
	{
		get => (double) GetValue(Heading3MarginProperty);
		set => SetValue(Heading3MarginProperty, value);
	}

	/// <summary>
	/// Font family used in level 4 heading blocks.
	/// </summary>
	public FontFamily? Heading4FontFamily
	{
		get => (FontFamily) GetValue(Heading4FontFamilyProperty);
		set => SetValue(Heading4FontFamilyProperty, value);
	}

	/// <summary>
	/// Base font weight used in level 4 heading blocks.
	/// </summary>
	public FontWeight Heading4FontWeight
	{
		get => (FontWeight) GetValue(Heading4FontWeightProperty);
		set => SetValue(Heading4FontWeightProperty, value);
	}

	/// <summary>
	/// Font size used in level 4 heading blocks.
	/// </summary>
	public double Heading4FontSize
	{
		get => (double) GetValue(Heading4FontSizeProperty);
		set => SetValue(Heading4FontSizeProperty, value);
	}

	/// <summary>
	/// Line height used in level 4 heading blocks.
	/// </summary>
	public double Heading4LineHeight
	{
		get => (double) GetValue(Heading4LineHeightProperty);
		set => SetValue(Heading4LineHeightProperty, value);
	}

	/// <summary>
	/// The brush used to render the foreground in level 4 heading blocks.
	/// </summary>
	public Brush? Heading4Foreground
	{
		get => (Brush) GetValue(Heading4ForegroundProperty);
		set => SetValue(Heading4ForegroundProperty, value);
	}

	/// <summary>
	/// Margin displayed around a level 4 heading block.
	/// </summary>
	public double Heading4Margin
	{
		get => (double) GetValue(Heading4MarginProperty);
		set => SetValue(Heading4MarginProperty, value);
	}

	/// <summary>
	/// Font family used in level 5 heading blocks.
	/// </summary>
	public FontFamily? Heading5FontFamily
	{
		get => (FontFamily) GetValue(Heading5FontFamilyProperty);
		set => SetValue(Heading5FontFamilyProperty, value);
	}

	/// <summary>
	/// Base font weight used in level 5 heading blocks.
	/// </summary>
	public FontWeight Heading5FontWeight
	{
		get => (FontWeight) GetValue(Heading5FontWeightProperty);
		set => SetValue(Heading5FontWeightProperty, value);
	}

	/// <summary>
	/// Font size used in level 5 heading blocks.
	/// </summary>
	public double Heading5FontSize
	{
		get => (double) GetValue(Heading5FontSizeProperty);
		set => SetValue(Heading5FontSizeProperty, value);
	}

	/// <summary>
	/// Line height used in level 5 heading blocks.
	/// </summary>
	public double Heading5LineHeight
	{
		get => (double) GetValue(Heading5LineHeightProperty);
		set => SetValue(Heading5LineHeightProperty, value);
	}

	/// <summary>
	/// The brush used to render the foreground in level 5 heading blocks.
	/// </summary>
	public Brush? Heading5Foreground
	{
		get => (Brush) GetValue(Heading5ForegroundProperty);
		set => SetValue(Heading5ForegroundProperty, value);
	}

	/// <summary>
	/// Margin displayed around a level 5 heading block.
	/// </summary>
	public double Heading5Margin
	{
		get => (double) GetValue(Heading5MarginProperty);
		set => SetValue(Heading5MarginProperty, value);
	}

	/// <summary>
	/// Font family used in level 6 heading blocks.
	/// </summary>
	public FontFamily? Heading6FontFamily
	{
		get => (FontFamily) GetValue(Heading6FontFamilyProperty);
		set => SetValue(Heading6FontFamilyProperty, value);
	}

	/// <summary>
	/// Base font weight used in level 6 heading blocks.
	/// </summary>
	public FontWeight Heading6FontWeight
	{
		get => (FontWeight) GetValue(Heading6FontWeightProperty);
		set => SetValue(Heading6FontWeightProperty, value);
	}

	/// <summary>
	/// Font size used in level 6 heading blocks.
	/// </summary>
	public double Heading6FontSize
	{
		get => (double) GetValue(Heading6FontSizeProperty);
		set => SetValue(Heading6FontSizeProperty, value);
	}

	/// <summary>
	/// Line height used in level 6 heading blocks.
	/// </summary>
	public double Heading6LineHeight
	{
		get => (double) GetValue(Heading6LineHeightProperty);
		set => SetValue(Heading6LineHeightProperty, value);
	}

	/// <summary>
	/// The brush used to render the foreground in level 6 heading blocks.
	/// </summary>
	public Brush? Heading6Foreground
	{
		get => (Brush) GetValue(Heading6ForegroundProperty);
		set => SetValue(Heading6ForegroundProperty, value);
	}

	/// <summary>
	/// Margin displayed around a level 6 heading block.
	/// </summary>
	public double Heading6Margin
	{
		get => (double) GetValue(Heading6MarginProperty);
		set => SetValue(Heading6MarginProperty, value);
	}

	/// <summary>
	/// Size of a divider in pixels.
	/// </summary>
	public double DividerThickness
	{
		get => (double) GetValue(DividerThicknessProperty);
		set => SetValue(DividerThicknessProperty, value);
	}

	/// <summary>
	/// Brush used to render dividers.
	/// </summary>
	public Brush? DividerBrush
	{
		get => (Brush) GetValue(DividerBrushProperty);
		set => SetValue(DividerBrushProperty, value);
	}

	/// <summary>
	/// Margin displayed around dividers.
	/// </summary>
	public double DividerMargin
	{
		get => (double) GetValue(DividerMarginProperty);
		set => SetValue(DividerMarginProperty, value);
	}

	/// <summary>
	/// The end position of the list bullet.
	/// </summary>
	public double ListBulletIndent
	{
		get => (double) GetValue(ListBulletIndentProperty);
		set => SetValue(ListBulletIndentProperty, value);
	}

	/// <summary>
	/// Spacing between list bullet and content.
	/// </summary>
	public double ListBulletSpacing
	{
		get => (double) GetValue(ListBulletSpacingProperty);
		set => SetValue(ListBulletSpacingProperty, value);
	}

	/// <summary>
	/// Margin displayed around list blocks.
	/// </summary>
	public double ListMargin
	{
		get => (double) GetValue(ListMarginProperty);
		set => SetValue(ListMarginProperty, value);
	}

	/// <summary>
	/// Indentation at the start of a quote block.
	/// </summary>
	public double QuoteIndent
	{
		get => (double) GetValue(QuoteIndentProperty);
		set => SetValue(QuoteIndentProperty, value);
	}

	/// <summary>
	/// The width of the indicator displayed next to a quote.
	/// </summary>
	public double QuoteIndicatorThickness
	{
		get => (double) GetValue(QuoteIndicatorThicknessProperty);
		set => SetValue(QuoteIndicatorThicknessProperty, value);
	}

	/// <summary>
	/// The brush used to render the indicator next to a quote.
	/// </summary>
	public Brush? QuoteIndicatorBrush
	{
		get => (Brush) GetValue(QuoteIndicatorBrushProperty);
		set => SetValue(QuoteIndicatorBrushProperty, value);
	}

	/// <summary>
	/// Margin displayed around quote blocks.
	/// </summary>
	public double QuoteMargin
	{
		get => (double) GetValue(QuoteMarginProperty);
		set => SetValue(QuoteMarginProperty, value);
	}

	/// <summary>
	/// Font weight used in table header cells.
	/// </summary>
	public FontWeight TableHeaderFontWeight
	{
		get => (FontWeight) GetValue(TableHeaderFontWeightProperty);
		set => SetValue(TableHeaderFontWeightProperty, value);
	}

	/// <summary>
	/// The background displayed behind a table header cell.
	/// </summary>
	public Brush? TableHeaderBackground
	{
		get => (Brush) GetValue(TableHeaderBackgroundProperty);
		set => SetValue(TableHeaderBackgroundProperty, value);
	}

	/// <summary>
	/// Foreground used in table header cells.
	/// </summary>
	public Brush? TableHeaderForeground
	{
		get => (Brush) GetValue(TableHeaderForegroundProperty);
		set => SetValue(TableHeaderForegroundProperty, value);
	}

	/// <summary>
	/// Inset used for table cells.
	/// </summary>
	public Thickness TablePadding
	{
		get => (Thickness) GetValue(TablePaddingProperty);
		set => SetValue(TablePaddingProperty, value);
	}

	/// <summary>
	/// Margin displayed around table blocks.
	/// </summary>
	public double TableMargin
	{
		get => (double) GetValue(TableMarginProperty);
		set => SetValue(TableMarginProperty, value);
	}

	private void RenderText()
	{
		if (_container == null)
			return;

		_container.Children.Clear();
		if (RichText != null)
		{
			var renderer = new BlockRenderer(this, _container, new BlockRenderingContext());
			foreach (var block in RichText.Blocks)
				block.Accept(renderer);
		}
	}

	protected override void OnApplyTemplate()
	{
		_container = GetTemplateChild("Container") as StackPanel;
		RenderText();
	}

	private static void HandlePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e) =>
		((RichTextViewer) sender).RenderText();

	private StackPanel? _container;
}
