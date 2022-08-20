using System;
using FluentFeeds.App.WinUI.Helpers;
using FluentFeeds.Common;
using Microsoft.UI.Xaml.Data;

namespace FluentFeeds.App.WinUI.Converters;

/// <summary>
/// Value converter calling <see cref="SymbolHelper.ToIconElement(Symbol)"/>.
/// </summary>
public sealed class SymbolToIconElementConverter : IValueConverter
{
	public object? Convert(object? value, Type targetType, object parameter, string language) =>
		((Symbol?)value)?.ToIconElement();

	public object? ConvertBack(object? value, Type targetType, object parameter, string language) =>
		throw new NotSupportedException();
}
