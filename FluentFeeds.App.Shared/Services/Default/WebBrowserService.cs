using System;
using System.Diagnostics;

namespace FluentFeeds.App.Shared.Services.Default;

public sealed class WebBrowserService : IWebBrowserService
{
	public void Open(Uri url)
	{
		Process.Start(new ProcessStartInfo { UseShellExecute = true, FileName = url.ToString() });
	}
}
