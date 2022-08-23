using System;
using Microsoft.UI.Xaml.Data;

namespace FluentFeeds.App.WinUI.Converters;

/// <summary>
/// Value converter converting <c>DateTimeOffset</c>s to human-readable strings.
/// </summary>
public sealed class TimestampToStringConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, string language)
	{
		var localTimestamp = ((DateTimeOffset)value).LocalDateTime;
		if (localTimestamp.Date == DateTime.Today)
		{
			// Only show time
			return localTimestamp.ToString("t");
		}

		// Show the full date
		return localTimestamp.ToString("d");
	}

	public object ConvertBack(object value, Type targetType, object parameter, string language) =>
		throw new NotSupportedException();
}
