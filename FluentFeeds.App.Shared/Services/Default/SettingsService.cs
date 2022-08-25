using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;
using FluentFeeds.App.Shared.Helpers;
using FluentFeeds.App.Shared.Models;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace FluentFeeds.App.Shared.Services.Default;

public class SettingsService : ObservableObject, ISettingsService
{
	protected record SerializedSettings(
		FontFamily ContentFontFamily,
		FontSize ContentFontSize,
		Theme AppTheme);

	/// <summary>
	/// Load initial settings (if any).
	/// </summary>
	protected SettingsService(SerializedSettings? serializedSettings)
	{
		_serialized = serializedSettings ?? new SerializedSettings(
			ContentFontFamily: FontFamily.SansSerif,
			ContentFontSize: FontSize.Normal,
			AppTheme: Theme.SystemDefault);
	}

	public SettingsService() : this(Load())
	{
	}

	public FontFamily ContentFontFamily
	{
		get => _serialized.ContentFontFamily;
		set => Update(_serialized with { ContentFontFamily = value });
	}

	public FontSize ContentFontSize
	{
		get => _serialized.ContentFontSize;
		set => Update(_serialized with { ContentFontSize = value });
	}

	public Theme AppTheme
	{
		get => _serialized.AppTheme;
		set => Update(_serialized with { AppTheme = value });
	}

	private void Update(SerializedSettings updated, [CallerMemberName] string? updatedPropertyName = null)
	{
		if (updated != _serialized)
		{
			OnPropertyChanging(updatedPropertyName);
			_serialized = updated;
			Store(_serialized);
			OnPropertyChanged(updatedPropertyName);
		}
	}

	private static SerializedSettings? Load()
	{
		try
		{
			var path = AppData.GetPath("settings.json");
			if (File.Exists(path))
			{
				using var file = File.OpenRead(AppData.GetPath("settings.json"));
				return JsonSerializer.Deserialize<SerializedSettings>(file);
			}
		}
		catch (Exception)
		{
			// ignore
		}

		return null;
	}

	/// <summary>
	/// Store the updated settings.
	/// </summary>
	protected virtual void Store(SerializedSettings serializedSettings)
	{
		try
		{
			var path = AppData.GetPath("settings.json", ensureExists: false);
			using var file = File.Create(path);
			JsonSerializer.Serialize(file, serializedSettings);
		}
		catch (Exception)
		{
			// ignore
		}
	}

	private SerializedSettings _serialized;
}
