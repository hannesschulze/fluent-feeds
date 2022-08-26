using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace FluentFeeds.App.WinUI.Converters;

/// <summary>
/// Value converter converting a number value into a thickness.
/// </summary>
public sealed class NumberToThicknessConverter : IValueConverter
{
	/// <summary>
	/// The base thickness value that is multiplied with the input number.
	/// </summary>
	public Thickness BaseThickness { get; set; } = new(0.0);

	public object Convert(object? value, Type targetType, object parameter, string language)
	{
		var factor = (int?)value ?? 0;
		return new Thickness(
			BaseThickness.Left * factor,
			BaseThickness.Top * factor,
			BaseThickness.Right * factor,
			BaseThickness.Bottom * factor);
	}

	public object ConvertBack(object value, Type targetType, object parameter, string language) =>
		throw new NotSupportedException();
}
