﻿using System.ComponentModel;
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

	/// <summary>
	/// The selected application theme.
	/// </summary>
	Theme AppTheme { get; set; }

	/// <summary>
	/// Allows the user to enable/disable Hacker News integration.
	/// </summary>
	bool IsHackerNewsEnabled { get; set; }
}
