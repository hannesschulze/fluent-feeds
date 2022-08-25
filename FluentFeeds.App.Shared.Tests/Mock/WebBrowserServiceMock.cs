using System;
using FluentFeeds.App.Shared.Services;

namespace FluentFeeds.App.Shared.Tests.Mock;

public sealed class WebBrowserServiceMock : IWebBrowserService
{
	public sealed class OpenEventArgs : System.EventArgs
	{
		public OpenEventArgs(Uri url)
		{
			Url = url;
		}
		
		public Uri Url { get; }
	}

	public event EventHandler<OpenEventArgs>? OpenEvent; 

	public void Open(Uri url)
	{
		OpenEvent?.Invoke(this, new OpenEventArgs(url));
	}
}
