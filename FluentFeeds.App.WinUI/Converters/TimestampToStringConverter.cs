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
		return localTimestamp.ToString(localTimestamp.Date == DateTime.Today ? "t" : "d");
	}

	public object ConvertBack(object value, Type targetType, object parameter, string language) =>
		throw new NotSupportedException();
}
