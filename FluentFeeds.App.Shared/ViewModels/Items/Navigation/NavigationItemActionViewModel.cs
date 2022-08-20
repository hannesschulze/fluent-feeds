using System.Windows.Input;
using FluentFeeds.Common;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace FluentFeeds.App.Shared.ViewModels.Items.Navigation;

/// <summary>
/// An action which can be executed on a navigation item.
/// </summary>
public sealed class NavigationItemActionViewModel : ObservableObject
{
	public NavigationItemActionViewModel(ICommand command, string title, Symbol? symbol)
	{
		Command = command;
		Title = title;
		Symbol = symbol;
	}

	/// <summary>
	/// The command executed for this action.
	/// </summary>
	public ICommand Command { get; }

	/// <summary>
	/// The title for this action.
	/// </summary>
	public string Title { get; }

	/// <summary>
	/// Symbol displayed next to the action item.
	/// </summary>
	public Symbol? Symbol { get; }
}
