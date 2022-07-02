using System;
using System.Linq;
using System.Collections.Generic;
using FluentFeeds.Shared.RichText.Inlines;
using Microsoft.UI.Xaml.Media;
using Windows.UI;
using Windows.UI.Text;
using MUXD = Microsoft.UI.Xaml.Documents;
using System.IO;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;

namespace FluentFeeds.WinUI.Controls;

public partial class RichTextViewer
{
	private readonly record struct InlineRenderingContext()
	{
		public InlineRenderer? Parent { get; init; } = null;
		public Func<MUXD.Span>? CreateSpan { get; init; } = null;

		public bool IsBold { get; init; } = false;
		public bool IsItalic { get; init; } = false;
		public bool IsUnderline { get; init; } = false;
		public bool IsStrikethrough { get; init; } = false;
		public bool IsCode { get; init; } = false;
		public bool IsHyperlink { get; init; } = false;
		public Uri? HyperlinkUri { get; init; } = null;
	}

	/// <summary>
	/// Renders inlines inside a rich text block.
	/// </summary>
	private sealed class InlineRenderer : IInlineVisitor
	{
		/// <summary>
		/// Create a new inline renderer.
		/// </summary>
		/// <param name="container">The container into which the inlines should be rendered.</param>
		public InlineRenderer(MUXD.InlineCollection container, in InlineRenderingContext context)
		{
			Container = container;
			Context = context;
		}

		public MUXD.InlineCollection Container { get; set; }
		
		public InlineRenderingContext Context { get; }

		public void Visit(TextInline inline)
		{
			Container.Add(new MUXD.Run { Text = inline.Text });
		}

		public void Visit(ImageInline inline)
		{
			if (inline.Source != null)
			{
				var image =
					new Image 
					{ 
						Source = Path.GetExtension(inline.Source.AbsolutePath)?.ToLowerInvariant() == ".svg"
							? new SvgImageSource(inline.Source) : new BitmapImage(inline.Source),
						Width = inline.Width >= 0 ? inline.Width : Double.NaN,
						Height = inline.Height >= 0 ? inline.Height : Double.NaN
					};
				if (inline.Width >= 0 && inline.Height >= 0)
					image.Stretch = Stretch.Fill;
				else if (inline.Width >= 0 || inline.Height >= 0)
					image.Stretch = Stretch.Uniform;
				else
					image.Stretch = Stretch.None;

				ToolTipService.SetToolTip(image, inline.AlternateText);

				if (Context.IsHyperlink)
				{
					var hyperlink =
						new HyperlinkButton
						{
							NavigateUri = Context.HyperlinkUri,
							Content = image,
							Padding = new Thickness(0, 0, 0, 0)
						};
					InsertSpecialElement(new MUXD.InlineUIContainer { Child = hyperlink });
				}
				else
				{
					InsertSpecialElement(new MUXD.InlineUIContainer { Child = image });
				}
			}
			else
			{
				Container.Add(new MUXD.Run { Text = inline.AlternateText ?? "" });
			}
		}

		public void Visit(BoldInline inline)
		{
			if (SkipRedundantSpan(Context.IsBold, inline))
				return;

			WrapSpan(inline, () => new MUXD.Bold(), Context with { IsBold = true });
		}

		public void Visit(ItalicInline inline)
		{
			if (SkipRedundantSpan(Context.IsItalic, inline))
				return;

			WrapSpan(inline, () => new MUXD.Italic(), Context with { IsItalic = true });
		}

		public void Visit(UnderlineInline inline)
		{
			if (SkipRedundantSpan(Context.IsUnderline, inline))
				return;

			WrapSpan(inline, () => new MUXD.Underline(), Context with { IsUnderline = true });
		}

		public void Visit(StrikethroughInline inline)
		{
			if (SkipRedundantSpan(Context.IsStrikethrough, inline))
				return;

			WrapSpan(
				inline, () => new MUXD.Span { TextDecorations = TextDecorations.Strikethrough }, 
				Context with { IsStrikethrough = true });
		}

		public void Visit(CodeInline inline)
		{
			if (SkipRedundantSpan(Context.IsCode, inline))
				return;

			WrapSpan(
				inline, () => new MUXD.Span { Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 80, 90)) },
				Context with { IsCode = true });
		}

		public void Visit(HyperlinkInline inline)
		{
			if (SkipRedundantSpan(Context.IsHyperlink && Context.HyperlinkUri == inline.Target, inline))
				return;

			// TODO: Allow nested hyperlinks
			WrapSpan(
				inline, () => new MUXD.Hyperlink { NavigateUri = inline.Target }, 
				Context with { IsHyperlink = true, HyperlinkUri = inline.Target });
		}

		private bool SkipRedundantSpan(bool isRedundant, SpanInline span)
		{
			if (isRedundant)
			{
				foreach (var inline in span.Inlines)
				{
					inline.Accept(this);
				}
			}

			return isRedundant;
		}

		/// <summary>
		/// <para>Insert a special element which cannot be added to a hyperlink inline.</para>
		/// 
		/// <para>This splits the hyperlink and all nested elements at the current position and inserts the provided 
		/// inline. This is necessary to insert UI elements or nested hyperlinks (otherwise Microsoft.UI.Xaml.Documents
		/// throws an exception).</para>
		/// </summary>
		/// <returns>The collection into which the inline was inserted.</returns>
		private MUXD.InlineCollection InsertSpecialElement(MUXD.Inline inline)
		{
			InlineRenderer renderer = this;
			MUXD.Span? currentElement = null;
			while (true)
			{
				if (!renderer.Context.IsHyperlink)
				{
					renderer.Container.Add(inline);
					if (currentElement != null)
						renderer.Container.Add(currentElement);
					return renderer.Container;
				}

				if (renderer.Context.Parent == null || renderer.Context.CreateSpan == null)
				{
					throw new InvalidOperationException(
						"Cannot insert special element because the top-level context is a hyperlink.");
				}

				var newElement = renderer.Context.CreateSpan();
				if (currentElement != null)
					newElement.Inlines.Add(currentElement);
				currentElement = newElement;
				renderer.Container = newElement.Inlines;
				renderer = renderer.Context.Parent;
			}
		}

		private void WrapSpan(SpanInline span, Func<MUXD.Span> createSpan, in InlineRenderingContext context)
		{
			var firstSpan = createSpan();
			Container.Add(firstSpan);
			var renderer = new InlineRenderer(
				firstSpan.Inlines, context with { Parent = this, CreateSpan = createSpan });
			foreach (var inline in span.Inlines)
				inline.Accept(renderer);
		}
	}
}
