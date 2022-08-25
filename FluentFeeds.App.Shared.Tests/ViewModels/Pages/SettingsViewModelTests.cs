using System;
using FluentFeeds.App.Shared.Models;
using FluentFeeds.App.Shared.Tests.Mock;
using FluentFeeds.App.Shared.ViewModels.Pages;
using Xunit;

namespace FluentFeeds.App.Shared.Tests.ViewModels.Pages;

public class SettingsViewModelTests
{
	private SettingsServiceMock SettingsService { get; } = new();

	private WebBrowserServiceMock WebBrowserService { get; } = new();
	
	[Fact]
	public void UpdateAppTheme()
	{
		var viewModel = new SettingsViewModel(SettingsService, WebBrowserService);
		Assert.Equal(Theme.SystemDefault, viewModel.SelectedTheme);
		SettingsService.AppTheme = Theme.Light;
		Assert.Equal(Theme.Light, viewModel.SelectedTheme);
		viewModel.SelectedTheme = Theme.Dark;
		Assert.Equal(Theme.Dark, SettingsService.AppTheme);
	}

	[Fact]
	public void Commands_OpenProjectWebsite()
	{
		var viewModel = new SettingsViewModel(SettingsService, WebBrowserService);
		var openArgs = Assert.Raises<WebBrowserServiceMock.OpenEventArgs>(
			h => WebBrowserService.OpenEvent += h, h => WebBrowserService.OpenEvent -= h,
			() => viewModel.OpenProjectWebsiteCommand.Execute(null)).Arguments;
		Assert.Equal(new Uri(Constants.AppWebsiteUrl), openArgs.Url);
	}

	[Fact]
	public void Commands_OpenGitHubIssues()
	{
		var viewModel = new SettingsViewModel(SettingsService, WebBrowserService);
		var openArgs = Assert.Raises<WebBrowserServiceMock.OpenEventArgs>(
			h => WebBrowserService.OpenEvent += h, h => WebBrowserService.OpenEvent -= h,
			() => viewModel.OpenGitHubIssuesCommand.Execute(null)).Arguments;
		Assert.Equal(new Uri(Constants.AppIssuesUrl), openArgs.Url);
	}
}
