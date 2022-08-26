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
	private static readonly FontFamily FontFamily_Tahoma = new("Tahoma");

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
			// Glyphs values from:
			// https://github.com/microsoft/fluentui-system-icons/blob/f54ade5c858c4577c3fe7dbaaa2551ddd086e592/fonts/FluentSystemIcons-Resizable.json
			Symbol.Home => CreateFont("\uE77E", FontFamily_FluentSystemIcons_Resizable),
			Symbol.Sparkle => CreateFont("\uEC10", FontFamily_FluentSystemIcons_Resizable),
			Symbol.Feed => CreateFont("\uEB08", FontFamily_FluentSystemIcons_Resizable),
			Symbol.Directory => CreateFont("\uE6AD", FontFamily_FluentSystemIcons_Resizable),
			Symbol.Web => CreateFont("\uE71F", FontFamily_FluentSystemIcons_Resizable),
			Symbol.Synchronize => CreateFont("\uE11D", FontFamily_FluentSystemIcons_Resizable),
			Symbol.Refresh => CreateFont("\uE095", FontFamily_FluentSystemIcons_Resizable),
			Symbol.MailUnread => CreateFont("\uE883", FontFamily_FluentSystemIcons_Resizable),
			Symbol.MailRead => CreateFont("\uE879", FontFamily_FluentSystemIcons_Resizable),
			Symbol.Trash => CreateFont("\uE4D2", FontFamily_FluentSystemIcons_Resizable),
			Symbol.SortOrder => CreateFont("\uE103", FontFamily_FluentSystemIcons_Resizable),
			Symbol.OpenExternal => CreateFont("\uE92D", FontFamily_FluentSystemIcons_Resizable),
			Symbol.Font or Symbol.FontFamily => CreateFont("\uEDEA", FontFamily_FluentSystemIcons_Resizable),
			Symbol.FontSizeIncrease => CreateFont("\uE6D1", FontFamily_FluentSystemIcons_Resizable),
			Symbol.FontSizeDecrease => CreateFont("\uE6CF", FontFamily_FluentSystemIcons_Resizable),
			Symbol.FontSizeReset => CreateFont("\uE09B", FontFamily_FluentSystemIcons_Resizable),
			Symbol.ColorPalette => CreateFont("\uE3FF", FontFamily_FluentSystemIcons_Resizable),
			Symbol.Search => CreateFont("\uEB48", FontFamily_FluentSystemIcons_Resizable),
			Symbol.HackerNews => CreateFont("Y", FontFamily_Tahoma),
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
