using FluentFeeds.Shared.ViewModels;
using Xunit;

namespace FluentFeeds.Shared.Tests;

public class MainViewModelTests
{
	public MainViewModel ViewModel { get; } = new();

	[Fact]
	public void ClickUpdatesLabel()
	{
		Assert.Equal("Clicked 0 times.", ViewModel.Label);
		ViewModel.ClickCommand.Execute(null);
		Assert.Equal("Clicked 1 times.", ViewModel.Label);
	}
}
