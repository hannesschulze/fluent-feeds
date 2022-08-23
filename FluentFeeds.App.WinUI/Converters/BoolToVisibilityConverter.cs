using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace FluentFeeds.App.WinUI.Converters;

/// <summary>
/// Value converter transforming booleans into WinUI <c>Visibility</c> enum values.
/// </summary>
public sealed class BoolToVisibilityConverter : IValueConverter
{
	public bool IsInverted { get; set; }

	public object Convert(object value, Type targetType, object parameter, string language) =>
		((bool)value) == !IsInverted ? Visibility.Visible : Visibility.Collapsed;

	public object ConvertBack(object value, Type targetType, object parameter, string language) =>
		throw new NotSupportedException();
}
