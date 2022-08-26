using System.Globalization;
using System.Threading;
using FluentFeeds.App.Shared.Models.Navigation;
using FluentFeeds.App.Shared.ViewModels.Pages;
using Xunit;

namespace FluentFeeds.App.Shared.Tests.ViewModels.Pages;

public sealed class SelectionViewModelTests
{
	public SelectionViewModelTests()
	{
		Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
	}
	
	[Fact]
	public void NoneSelected()
	{
		var viewModel = new SelectionViewModel();
		viewModel.Load(FeedNavigationRoute.Selection(count: 0));
		Assert.Equal("No items selected", viewModel.Title);
		Assert.True(viewModel.IsMessageVisible);
		Assert.Equal("The content of the selected item will be shown here.", viewModel.Message);
	}

	[Fact]
	public void MultipleSelected()
	{
		var viewModel = new SelectionViewModel();
		viewModel.Load(FeedNavigationRoute.Selection(count: 2));
		Assert.Equal("2 items selected", viewModel.Title);
		Assert.False(viewModel.IsMessageVisible);
	}
}
