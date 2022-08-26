using System;
using FluentFeeds.Documents;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace FluentFeeds.App.WinUI.Controls;

/// <summary>
/// A rich text viewer container which can apply a specified font family and font size.
/// </summary>
public sealed partial class StyledRichTextViewer : UserControl
{
	public static readonly DependencyProperty RichTextProperty = DependencyProperty.Register(
		nameof(RichText), typeof(RichText), typeof(StyledRichTextViewer),
		new PropertyMetadata(null, HandleRichTextChanged));
	public static readonly DependencyProperty StyledFontFamilyProperty = DependencyProperty.Register(
		nameof(StyledFontFamily), typeof(Shared.Models.FontFamily), typeof(StyledRichTextViewer),
		new PropertyMetadata(Shared.Models.FontFamily.SansSerif, HandleFontFamilyChanged));
	public static readonly DependencyProperty StyledFontSizeProperty = DependencyProperty.Register(
		nameof(StyledFontSize), typeof(Shared.Models.FontSize), typeof(StyledRichTextViewer),
		new PropertyMetadata(Shared.Models.FontSize.Normal, HandleFontSizeChanged));

	public StyledRichTextViewer()
	{
		InitializeComponent();

		UpdateFontFamily();
		UpdateFontSize();
	}

	/// <summary>
	/// The text content displayed in this control.
	/// </summary>
	public RichText? RichText
	{
		get => (RichText)GetValue(RichTextProperty);
		set => SetValue(RichTextProperty, value);
	}

	/// <summary>
	/// The font family, selected from a range of predefined font families.
	/// </summary>
	public Shared.Models.FontFamily StyledFontFamily
	{
		get => (Shared.Models.FontFamily)GetValue(StyledFontFamilyProperty);
		set => SetValue(StyledFontFamilyProperty, value);
	}

	/// <summary>
	/// The font size, selected from a range of predefined font sizes.
	/// </summary>
	public Shared.Models.FontSize StyledFontSize
	{
		get => (Shared.Models.FontSize)GetValue(StyledFontSizeProperty);
		set => SetValue(StyledFontSizeProperty, value);
	}

	private void UpdateRichText()
	{
		ContentViewer.RichText = RichText;
	}

	private void UpdateFontSize()
	{
		var fontSize = StyledFontSize;

		ContentViewer.BodyFontSize = ContentViewer.CodeFontSize =
			fontSize switch
			{
				Shared.Models.FontSize.Small => 12,
				Shared.Models.FontSize.Normal => 14,
				Shared.Models.FontSize.Large => 16,
				Shared.Models.FontSize.ExtraLarge => 20,
				_ => throw new IndexOutOfRangeException()
			};
		ContentViewer.BodyLineHeight = ContentViewer.CodeLineHeight =
			fontSize switch
			{
				Shared.Models.FontSize.Small => 16,
				Shared.Models.FontSize.Normal => 20,
				Shared.Models.FontSize.Large => 22,
				Shared.Models.FontSize.ExtraLarge => 26,
				_ => throw new IndexOutOfRangeException()
			};
		ContentViewer.Heading1FontSize =
			fontSize switch
			{
				Shared.Models.FontSize.Small => 24,
				Shared.Models.FontSize.Normal => 28,
				Shared.Models.FontSize.Large => 32,
				Shared.Models.FontSize.ExtraLarge => 36,
				_ => throw new IndexOutOfRangeException()
			};
		ContentViewer.Heading1LineHeight =
			fontSize switch
			{
				Shared.Models.FontSize.Small => 32,
				Shared.Models.FontSize.Normal => 36,
				Shared.Models.FontSize.Large => 38,
				Shared.Models.FontSize.ExtraLarge => 42,
				_ => throw new IndexOutOfRangeException()
			};
		ContentViewer.Heading2FontSize =
			fontSize switch
			{
				Shared.Models.FontSize.Small => 18,
				Shared.Models.FontSize.Normal => 21,
				Shared.Models.FontSize.Large => 24,
				Shared.Models.FontSize.ExtraLarge => 26,
				_ => throw new IndexOutOfRangeException()
			};
		ContentViewer.Heading2LineHeight =
			fontSize switch
			{
				Shared.Models.FontSize.Small => 24,
				Shared.Models.FontSize.Normal => 28,
				Shared.Models.FontSize.Large => 28,
				Shared.Models.FontSize.ExtraLarge => 30,
				_ => throw new IndexOutOfRangeException()
			};
		ContentViewer.Heading3FontSize =
			fontSize switch
			{
				Shared.Models.FontSize.Small => 14,
				Shared.Models.FontSize.Normal => 16,
				Shared.Models.FontSize.Large => 18,
				Shared.Models.FontSize.ExtraLarge => 22,
				_ => throw new IndexOutOfRangeException()
			};
		ContentViewer.Heading3LineHeight =
			fontSize switch
			{
				Shared.Models.FontSize.Small => 18,
				Shared.Models.FontSize.Normal => 22,
				Shared.Models.FontSize.Large => 24,
				Shared.Models.FontSize.ExtraLarge => 28,
				_ => throw new IndexOutOfRangeException()
			};
		ContentViewer.Heading4FontSize =
			fontSize switch
			{
				Shared.Models.FontSize.Small => 12,
				Shared.Models.FontSize.Normal => 14,
				Shared.Models.FontSize.Large => 16,
				Shared.Models.FontSize.ExtraLarge => 20,
				_ => throw new IndexOutOfRangeException()
			};
		ContentViewer.Heading4LineHeight =
			fontSize switch
			{
				Shared.Models.FontSize.Small => 16,
				Shared.Models.FontSize.Normal => 20,
				Shared.Models.FontSize.Large => 22,
				Shared.Models.FontSize.ExtraLarge => 26,
				_ => throw new IndexOutOfRangeException()
			};
		ContentViewer.Heading5FontSize =
			fontSize switch
			{
				Shared.Models.FontSize.Small => 10,
				Shared.Models.FontSize.Normal => 12,
				Shared.Models.FontSize.Large => 14,
				Shared.Models.FontSize.ExtraLarge => 16,
				_ => throw new IndexOutOfRangeException()
			};
		ContentViewer.Heading5LineHeight =
			fontSize switch
			{
				Shared.Models.FontSize.Small => 12,
				Shared.Models.FontSize.Normal => 15,
				Shared.Models.FontSize.Large => 18,
				Shared.Models.FontSize.ExtraLarge => 20,
				_ => throw new IndexOutOfRangeException()
			};
		ContentViewer.Heading6FontSize =
			fontSize switch
			{
				Shared.Models.FontSize.Small => 9,
				Shared.Models.FontSize.Normal => 10,
				Shared.Models.FontSize.Large => 12,
				Shared.Models.FontSize.ExtraLarge => 14,
				_ => throw new IndexOutOfRangeException()
			};
		ContentViewer.Heading6LineHeight =
			fontSize switch
			{
				Shared.Models.FontSize.Small => 11,
				Shared.Models.FontSize.Normal => 12,
				Shared.Models.FontSize.Large => 16,
				Shared.Models.FontSize.ExtraLarge => 18,
				_ => throw new IndexOutOfRangeException()
			};
	}

	private void UpdateFontFamily()
	{
		var fontFamily =
			StyledFontFamily switch
			{
				Shared.Models.FontFamily.SansSerif => new FontFamily("Segoe UI"),
				Shared.Models.FontFamily.Serif => new FontFamily("Times New Roman"),
				Shared.Models.FontFamily.Monospace => new FontFamily("Consolas"),
				_ => throw new IndexOutOfRangeException()
			};
		ContentViewer.BodyFontFamily = fontFamily;
		ContentViewer.Heading1FontFamily = fontFamily;
		ContentViewer.Heading2FontFamily = fontFamily;
		ContentViewer.Heading3FontFamily = fontFamily;
		ContentViewer.Heading4FontFamily = fontFamily;
		ContentViewer.Heading5FontFamily = fontFamily;
		ContentViewer.Heading6FontFamily = fontFamily;
	}

	private static void HandleRichTextChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e) =>
		((StyledRichTextViewer) sender).UpdateRichText();

	private static void HandleFontFamilyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e) =>
		((StyledRichTextViewer)sender).UpdateFontFamily();

	private static void HandleFontSizeChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e) =>
		((StyledRichTextViewer)sender).UpdateFontSize();
}
