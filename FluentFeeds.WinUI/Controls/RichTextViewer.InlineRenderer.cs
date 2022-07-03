using System;
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
	/// <summary>
	/// Wrapper around a Microsoft.UI.Xaml.Documents inline and an inline collection inside it to allow layered
	/// spans to be used as one container (the span itself is different from the span hosting inner elements).
	/// </summary>
	private readonly record struct InlineContainer(MUXD.Inline OuterInline, MUXD.InlineCollection InnerContainer)
	{
		public InlineContainer(MUXD.Span span) : this(span, span.Inlines)
		{
		}
	}

	/// <summary>
	/// Current context of an <c>InlineRenderer</c> to avoid redundant inlines and make sure "special" elements are not
	/// used in a hyperlink.
	/// </summary>
	private readonly record struct InlineRenderingContext()
	{
		public InlineRenderer? Parent { get; init; } = null;
		public Func<InlineContainer>? CreateContainer { get; init; } = null;

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
		public InlineRenderer(
			RichTextViewer viewer, MUXD.InlineCollection container, in InlineRenderingContext context)
		{
			Container = container;
			Context = context;
			Viewer = viewer;
		}

		public MUXD.InlineCollection Container { get; set; }
		
		public InlineRenderingContext Context { get; }

		public RichTextViewer Viewer { get; }

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
					InsertSpecialElement(parent =>
						parent.Container.Add(new MUXD.InlineUIContainer { Child = hyperlink }));
				}
				else
				{
					InsertSpecialElement(parent =>
						parent.Container.Add(new MUXD.InlineUIContainer { Child = image }));
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

			WrapSpan(inline, () => new(CreateBoldSpan()), Context with { IsBold = true });
		}

		public void Visit(ItalicInline inline)
		{
			if (SkipRedundantSpan(Context.IsItalic, inline))
				return;

			WrapSpan(inline, () => new(CreateItalicSpan()), Context with { IsItalic = true });
		}

		public void Visit(UnderlineInline inline)
		{
			if (SkipRedundantSpan(Context.IsUnderline, inline))
				return;

			WrapSpan(inline, () => new(CreateUnderlineSpan()), Context with { IsUnderline = true });
		}

		public void Visit(StrikethroughInline inline)
		{
			if (SkipRedundantSpan(Context.IsStrikethrough, inline))
				return;

			WrapSpan(inline, () => new(CreateStrikethroughSpan()), Context with { IsStrikethrough = true });
		}

		public void Visit(CodeInline inline)
		{
			if (SkipRedundantSpan(Context.IsCode, inline))
				return;

			WrapSpan(inline, () => new(CreateCodeSpan()), Context with { IsCode = true });
		}

		public void Visit(HyperlinkInline inline)
		{
			if (SkipRedundantSpan(Context.IsHyperlink && Context.HyperlinkUri == inline.Target, inline))
				return;

			var innerContext = Context with { IsHyperlink = true, HyperlinkUri = inline.Target };
			InsertSpecialElement(
				parent =>
				{
					parent.WrapSpan(
						inline, () =>
						{
							var hyperlink = 
								new MUXD.Hyperlink 
								{
									NavigateUri = inline.Target, 
									Foreground = Viewer.HyperlinkForeground
								};
							// Restore outer format.
							var innerSpan = ApplyFormat(hyperlink, parent.Context, innerContext);
							return new(hyperlink, innerSpan.Inlines);
						}, innerContext);
				});
		}

		private static MUXD.Span CreateBoldSpan() =>
			new MUXD.Bold();

		private static MUXD.Span CreateItalicSpan() =>
			new MUXD.Italic();

		private static MUXD.Span CreateUnderlineSpan() =>
			new MUXD.Underline();

		private static MUXD.Span CreateStrikethroughSpan() =>
			new MUXD.Span { TextDecorations = TextDecorations.Strikethrough };

		private MUXD.Span CreateCodeSpan() =>
			new MUXD.Span { FontFamily = Viewer.CodeFontFamily };

		/// <summary>
		/// Apply a context's format inside a container with a different format. This ignores hyperlinks.
		/// </summary>
		private MUXD.Span ApplyFormat(
			MUXD.Span container, in InlineRenderingContext containerFormat, in InlineRenderingContext desiredFormat)
		{
			if (desiredFormat.IsBold && !containerFormat.IsBold)
			{
				var newContainer = CreateBoldSpan();
				container.Inlines.Add(newContainer);
				container = newContainer;
			}
			
			if (desiredFormat.IsItalic && !containerFormat.IsItalic)
			{
				var newContainer = CreateItalicSpan();
				container.Inlines.Add(newContainer);
				container = newContainer;
			}
			
			if (desiredFormat.IsUnderline && !containerFormat.IsUnderline)
			{
				var newContainer = CreateUnderlineSpan();
				container.Inlines.Add(newContainer);
				container = newContainer;
			}
			
			if (desiredFormat.IsStrikethrough && !containerFormat.IsStrikethrough)
			{
				var newContainer = CreateStrikethroughSpan();
				container.Inlines.Add(newContainer);
				container = newContainer;
			}
			
			if (desiredFormat.IsCode && !containerFormat.IsCode)
			{
				var newContainer = CreateCodeSpan();
				container.Inlines.Add(newContainer);
				container = newContainer;
			}

			return container;
		}

		/// <summary>
		/// If <c>isRedundant</c> is true, directly insert the contents of <c>span</c> into the current container.
		/// </summary>
		/// <returns>Same value as <c>isRedundant</c> to make chaining easier.</returns>
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
		private MUXD.InlineCollection InsertSpecialElement(Action<InlineRenderer> insertIntoParent)
		{
			InlineRenderer renderer = this;
			MUXD.Inline? currentElement = null;
			while (true)
			{
				if (!renderer.Context.IsHyperlink)
				{
					insertIntoParent(renderer);
					if (currentElement != null)
						renderer.Container.Add(currentElement);
					return renderer.Container;
				}

				if (renderer.Context.Parent == null || renderer.Context.CreateContainer == null)
				{
					throw new InvalidOperationException(
						"Cannot insert special element because the top-level context is a hyperlink.");
				}

				var container = renderer.Context.CreateContainer();
				if (currentElement != null)
					container.InnerContainer.Add(currentElement);
				currentElement = container.OuterInline;
				renderer.Container = container.InnerContainer;
				renderer = renderer.Context.Parent;
			}
		}

		/// <summary>
		/// Add the contents of <c>span</c> into the current container wrapped in the container created by 
		/// <c>createContainer</c>. <c>context</c> is the context of the new inline renderer used to render the inner 
		/// elements.
		/// </summary>
		private void WrapSpan(
			SpanInline span, Func<InlineContainer> createContainer, in InlineRenderingContext context)
		{
			var firstContainer = createContainer();
			Container.Add(firstContainer.OuterInline);
			var renderer = new InlineRenderer(
				Viewer, firstContainer.InnerContainer, 
				context with { Parent = this, CreateContainer = createContainer });
			foreach (var inline in span.Inlines)
				inline.Accept(renderer);
		}
	}
}
