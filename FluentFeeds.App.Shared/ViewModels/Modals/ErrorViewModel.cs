using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace FluentFeeds.App.Shared.ViewModels.Modals;

/// <summary>
/// View-model for displaying information about an error.
/// </summary>
public sealed class ErrorViewModel : ObservableObject
{
	public ErrorViewModel(string title, string message)
	{
		Title = title;
		Message = message;
	}

	/// <summary>
	/// Short description of the error.
	/// </summary>
	public string Title { get; }

	/// <summary>
	/// Explanation of the error.
	/// </summary>
	public string Message { get; }
}
