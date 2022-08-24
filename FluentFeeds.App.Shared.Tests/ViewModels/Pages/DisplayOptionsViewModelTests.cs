using FluentFeeds.App.Shared.Models;
using FluentFeeds.App.Shared.Tests.Mock;
using FluentFeeds.App.Shared.ViewModels.Modals;
using Xunit;

namespace FluentFeeds.App.Shared.Tests.ViewModels.Pages;

public class DisplayOptionsViewModelTests
{
	private SettingsServiceMock SettingsService { get; } = new();
	
	[Fact]
	public void AdjustFontFamily()
	{
		var viewModel = new DisplayOptionsViewModel(SettingsService);
		Assert.Equal(FontFamily.SansSerif, viewModel.SelectedFontFamily);
		SettingsService.ContentFontFamily = FontFamily.Serif;
		Assert.Equal(FontFamily.Serif, viewModel.SelectedFontFamily);
		viewModel.SelectedFontFamily = FontFamily.Monospace;
		Assert.Equal(FontFamily.Monospace, viewModel.SelectedFontFamily);
		Assert.Equal(FontFamily.Monospace, SettingsService.ContentFontFamily);
	}

	[Theory]
	[InlineData(FontSize.Small, true, false, true)]
	[InlineData(FontSize.Normal, true, true, false)]
	[InlineData(FontSize.Large, true, true, true)]
	[InlineData(FontSize.ExtraLarge, false, true, true)]
	public void AdjustFontSize_AvailableCommands(FontSize fontSize, bool canIncrease, bool canDecrease, bool canReset)
	{
		var viewModel = new DisplayOptionsViewModel(SettingsService);
		SettingsService.ContentFontSize = fontSize;
		Assert.Equal(canIncrease, viewModel.IncreaseFontSizeCommand.CanExecute(null));
		Assert.Equal(canDecrease, viewModel.DecreaseFontSizeCommand.CanExecute(null));
		Assert.Equal(canReset, viewModel.ResetFontSizeCommand.CanExecute(null));
	}

	[Theory]
	[InlineData(FontSize.Small, FontSize.Normal)]
	[InlineData(FontSize.Normal, FontSize.Large)]
	[InlineData(FontSize.Large, FontSize.ExtraLarge)]
	[InlineData(FontSize.ExtraLarge, FontSize.ExtraLarge)]
	public void AdjustFontSize_Increase(FontSize initialFontSize, FontSize expectedFontSize)
	{
		var viewModel = new DisplayOptionsViewModel(SettingsService);
		SettingsService.ContentFontSize = initialFontSize;
		viewModel.IncreaseFontSizeCommand.Execute(null);
		Assert.Equal(expectedFontSize, SettingsService.ContentFontSize);
	}

	[Theory]
	[InlineData(FontSize.Small, FontSize.Small)]
	[InlineData(FontSize.Normal, FontSize.Small)]
	[InlineData(FontSize.Large, FontSize.Normal)]
	[InlineData(FontSize.ExtraLarge, FontSize.Large)]
	public void AdjustFontSize_Decrease(FontSize initialFontSize, FontSize expectedFontSize)
	{
		var viewModel = new DisplayOptionsViewModel(SettingsService);
		SettingsService.ContentFontSize = initialFontSize;
		viewModel.DecreaseFontSizeCommand.Execute(null);
		Assert.Equal(expectedFontSize, SettingsService.ContentFontSize);
	}

	[Fact]
	public void AdjustFontSize_Reset()
	{
		var viewModel = new DisplayOptionsViewModel(SettingsService);
		SettingsService.ContentFontSize = FontSize.Large;
		viewModel.ResetFontSizeCommand.Execute(null);
		Assert.Equal(FontSize.Normal, SettingsService.ContentFontSize);
	}
}
