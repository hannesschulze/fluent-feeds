using System.Windows.Input;
using FluentFeeds.Common;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace FluentFeeds.App.Shared.ViewModels.Main;

/// <summary>
/// An action which can be executed on a list view item on the main page.
/// </summary>
public sealed class MainItemActionViewModel : ObservableObject
{
	public MainItemActionViewModel(ICommand command, string title, Symbol? symbol)
	{
		_command = command;
		_title = title;
		_symbol = symbol;
	}

	/// <summary>
	/// The command executed for this action.
	/// </summary>
	public ICommand Command
	{
		get => _command;
		set => SetProperty(ref _command, value);
	}

	/// <summary>
	/// The title for this action.
	/// </summary>
	public string Title
	{
		get => _title;
		set => SetProperty(ref _title, value);
	}

	/// <summary>
	/// Symbol displayed next to the action item.
	/// </summary>
	public Symbol? Symbol
	{
		get => _symbol;
		set => SetProperty(ref _symbol, value);
	}

	private ICommand _command;
	private string _title;
	private Symbol? _symbol;
}
