using System.ComponentModel;
using FluentFeeds.App.Shared.Models;

namespace FluentFeeds.App.Shared.Services;

/// <summary>
/// A service for persistently storing app settings.
/// </summary>
public interface ISettingsService : INotifyPropertyChanged
{
	/// <summary>
	/// Font size used to display content.
	/// </summary>
	FontSize ContentFontSize { get; set; }

	/// <summary>
	/// Font family used to display content.
	/// </summary>
	FontFamily ContentFontFamily { get; set; }
}
