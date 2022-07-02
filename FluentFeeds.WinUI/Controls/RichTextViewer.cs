using FluentFeeds.Shared.RichText;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Markup;
using Microsoft.UI.Xaml.Media;

namespace FluentFeeds.WinUI.Controls;

/// <summary>
/// A control which displays a generic <see cref="RichText"/> object.
/// </summary>
[ContentProperty(Name = "RichText")]
public sealed partial class RichTextViewer : Control
{
	public static readonly DependencyProperty RichTextProperty = DependencyProperty.Register(
		nameof(RichText), typeof(RichText), typeof(RichTextViewer), new PropertyMetadata(null, HandlePropertyChanged));

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

	private void RenderText()
	{
		if (_container == null)
			return;

		_container.Children.Clear();
		if (RichText != null)
		{
			var renderer = new BlockRenderer(_container);
			foreach (var block in RichText.Blocks)
				block.Accept(renderer);
		}
	}

	protected override void OnApplyTemplate()
	{
		_container = (StackPanel) GetTemplateChild("Container");
		RenderText();
	}

	private static void HandlePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e) =>
		((RichTextViewer) sender).RenderText();

	private StackPanel? _container;
}
