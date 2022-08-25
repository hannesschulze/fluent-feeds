using System.IO;
using System;

namespace FluentFeeds.App.Shared.Helpers;

public static class AppData
{
	private static readonly object Locker = new();

	/// <summary>
	/// Return a path in the app's app data directory, ensuring that the directory exists.
	/// </summary>
	public static string GetPath(string fileName, bool ensureExists = true)
	{
		var allAppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
		var appDataPath = Path.Combine(allAppDataPath, Constants.AppConfigName);
		if (ensureExists)
		{
			lock (Locker)
			{
				Directory.CreateDirectory(appDataPath);
			}
		}
		return Path.Combine(appDataPath, fileName);
	}
}
