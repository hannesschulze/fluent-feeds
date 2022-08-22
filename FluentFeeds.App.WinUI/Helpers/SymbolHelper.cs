using System;
using FluentFeeds.Common;
using Microsoft.UI.Xaml.Controls.AnimatedVisuals;
using Microsoft.UI.Xaml.Media;
using MUXC = Microsoft.UI.Xaml.Controls;

namespace FluentFeeds.App.WinUI.Helpers;

public static class SymbolHelper
{
	private static readonly FontFamily FontFamily_FluentSystemIcons_Resizable = 
		new("/Assets/FluentSystemIcons-Resizable.ttf#FluentSystemIcons-Resizable");

	private static MUXC.IconSource CreateFont(string glyph, FontFamily fontFamily) =>
		new MUXC.FontIconSource
		{
			FontFamily = fontFamily,
			IsTextScaleFactorEnabled = true,
			Glyph = glyph
		};

	private static MUXC.IconSource CreateAnimated(
		MUXC.IAnimatedVisualSource2 animatedSource, MUXC.IconSource fallback) =>
		new MUXC.AnimatedIconSource
		{
			Source = animatedSource,
			FallbackIconSource = fallback
		};

	public static MUXC.IconSource ToIconSource(this Symbol symbol) =>
		symbol switch
		{
			Symbol.Home => CreateFont("\xE77E", FontFamily_FluentSystemIcons_Resizable),
			Symbol.Sparkle => CreateFont("\xEC10", FontFamily_FluentSystemIcons_Resizable),
			Symbol.Feed => CreateFont("\xEB08", FontFamily_FluentSystemIcons_Resizable),
			Symbol.Directory => CreateFont("\xE6AD", FontFamily_FluentSystemIcons_Resizable),
			Symbol.Web => CreateFont("\xE71F", FontFamily_FluentSystemIcons_Resizable),
			Symbol.Settings => CreateAnimated(
				new AnimatedSettingsVisualSource(), 
				CreateFont("\xEB70", FontFamily_FluentSystemIcons_Resizable)),
			_ => throw new IndexOutOfRangeException()
		};

	public static MUXC.IconElement ToIconElement(this Symbol symbol)
	{
		var source = symbol.ToIconSource();
		if (source is MUXC.AnimatedIconSource animatedSource)
			return
				new MUXC.AnimatedIcon
				{
					Source = animatedSource.Source,
					FallbackIconSource = animatedSource.FallbackIconSource
				};

		return new MUXC.IconSourceElement { IconSource = source };
	}
}
