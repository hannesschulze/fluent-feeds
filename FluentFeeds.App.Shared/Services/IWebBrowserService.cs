using System;

namespace FluentFeeds.App.Shared.Services;

/// <summary>
/// Service for interacting with the user's default web browser.
/// </summary>
public interface IWebBrowserService
{
	/// <summary>
	/// Open a URL in the user's default web browser.
	/// </summary>
	public void Open(Uri url);
}
