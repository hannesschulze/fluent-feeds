using System;
using System.Windows.Input;
using FluentFeeds.App.Shared.Models;
using FluentFeeds.App.Shared.Resources;
using FluentFeeds.App.Shared.Services;
using FluentFeeds.Common;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;

namespace FluentFeeds.App.Shared.ViewModels.Modals;

/// <summary>
/// View model allowing the user to adjust options for viewing content.
/// </summary>
public sealed class DisplayOptionsViewModel : ObservableObject
{
	public DisplayOptionsViewModel(ISettingsService settingsService)
	{
		_settingsService = settingsService;

		_increaseFontSizeCommand = new RelayCommand(
			HandleIncreaseFontSizeCommand, () => _settingsService.ContentFontSize != FontSize.ExtraLarge);
		_decreaseFontSizeCommand = new RelayCommand(
			HandleDecreaseFontSizeCommand, () => _settingsService.ContentFontSize != FontSize.Small);
		_resetFontSizeCommand = new RelayCommand(
			HandleResetFontSizeCommand, () => _settingsService.ContentFontSize != FontSize.Normal);
		_settingsService.PropertyChanged += HandleSettingsChanged;
	}

	/// <summary>
	/// Increase the selected font size (if available).
	/// </summary>
	public ICommand IncreaseFontSizeCommand => _increaseFontSizeCommand;

	/// <summary>
	/// Decrease the selected font size (if available).
	/// </summary>
	public ICommand DecreaseFontSizeCommand => _decreaseFontSizeCommand;

	/// <summary>
	/// Reset the selected font size <see cref="FontSize.Normal"/>.
	/// </summary>
	public ICommand ResetFontSizeCommand => _resetFontSizeCommand;

	/// <summary>
	/// Symbol for the <see cref="SelectedFontFamily"/> menu.
	/// </summary>
	public Symbol FontFamilySymbol => Symbol.FontFamily;

	/// <summary>
	/// Symbol for <see cref="IncreaseFontSizeCommand"/>.
	/// </summary>
	public Symbol IncreaseFontSizeSymbol => Symbol.FontSizeIncrease;

	/// <summary>
	/// Symbol for <see cref="DecreaseFontSizeCommand"/>.
	/// </summary>
	public Symbol DecreaseFontSizeSymbol => Symbol.FontSizeDecrease;

	/// <summary>
	/// Symbol for <see cref="ResetFontSizeCommand"/>.
	/// </summary>
	public Symbol ResetFontSizeSymbol => Symbol.FontSizeReset;

	/// <summary>
	/// Label for the <see cref="SelectedFontFamily"/> menu.
	/// </summary>
	public string FontFamilyLabel => LocalizedStrings.DisplayOptionsFontFamilyLabel;

	/// <summary>
	/// Menu label for <see cref="FontFamily.SansSerif"/>.
	/// </summary>
	public string FontFamilySansSerifLabel => LocalizedStrings.DisplayOptionsFontFamilySansSerifLabel;

	/// <summary>
	/// Menu label for <see cref="FontFamily.Serif"/>.
	/// </summary>
	public string FontFamilySerifLabel => LocalizedStrings.DisplayOptionsFontFamilySerifLabel;
	
	/// <summary>
	/// Menu label for <see cref="FontFamily.Monospace"/>.
	/// </summary>
	public string FontFamilyMonospaceLabel => LocalizedStrings.DisplayOptionsFontFamilyMonospaceLabel;

	/// <summary>
	/// Label for <see cref="IncreaseFontSizeCommand"/>.
	/// </summary>
	public string IncreaseFontSizeLabel => LocalizedStrings.DisplayOptionsIncreaseFontSizeLabel;
	
	/// <summary>
	/// Label for <see cref="DecreaseFontSizeCommand"/>.
	/// </summary>
	public string DecreaseFontSizeLabel => LocalizedStrings.DisplayOptionsDecreaseFontSizeLabel;
	
	/// <summary>
	/// Label for <see cref="ResetFontSizeCommand"/>.
	/// </summary>
	public string ResetFontSizeLabel => LocalizedStrings.DisplayOptionsResetFontSizeLabel;

	/// <summary>
	/// Font family selection.
	/// </summary>
	public FontFamily SelectedFontFamily
	{
		get => _settingsService.ContentFontFamily;
		set => _settingsService.ContentFontFamily = value;
	}

	private void HandleIncreaseFontSizeCommand()
	{
		_settingsService.ContentFontSize =
			_settingsService.ContentFontSize switch
			{
				FontSize.Small => FontSize.Normal,
				FontSize.Normal => FontSize.Large,
				FontSize.Large or FontSize.ExtraLarge => FontSize.ExtraLarge,
				_ => throw new IndexOutOfRangeException()
			};
	}

	private void HandleDecreaseFontSizeCommand()
	{
		_settingsService.ContentFontSize =
			_settingsService.ContentFontSize switch
			{
				FontSize.Small or FontSize.Normal => FontSize.Small,
				FontSize.Large => FontSize.Normal,
				FontSize.ExtraLarge => FontSize.Large,
				_ => throw new IndexOutOfRangeException()
			};
	}

	private void HandleResetFontSizeCommand()
	{
		_settingsService.ContentFontSize = FontSize.Normal;
	}

	private void HandleSettingsChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
	{
		switch (e.PropertyName)
		{
			case nameof(ISettingsService.ContentFontFamily):
				OnPropertyChanged(nameof(SelectedFontFamily));
				break;
			case nameof(ISettingsService.ContentFontSize):
				_increaseFontSizeCommand.NotifyCanExecuteChanged();
				_decreaseFontSizeCommand.NotifyCanExecuteChanged();
				_resetFontSizeCommand.NotifyCanExecuteChanged();
				break;
		}
	}

	private readonly ISettingsService _settingsService;
	private readonly RelayCommand _increaseFontSizeCommand;
	private readonly RelayCommand _decreaseFontSizeCommand;
	private readonly RelayCommand _resetFontSizeCommand;
}
