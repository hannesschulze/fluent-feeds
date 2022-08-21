using FluentFeeds.App.Shared.ViewModels.Modals;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace FluentFeeds.App.WinUI.Views.Modals;

/// <summary>
/// Dialog presenting an <see cref="EditNodeViewModel"/>.
/// </summary>
public sealed partial class EditNodeView : ContentDialog
{
	public EditNodeView()
	{
		InitializeComponent();

		// XXX: ContentDialog's command handling seems buggy.
		Loading += HandleLoading;
	}

	public EditNodeViewModel ViewModel => (EditNodeViewModel)DataContext;

	private void HandleLoading(FrameworkElement sender, object args)
	{
		UpdateIsPrimaryButtonEnabled();
		ViewModel.SaveCommand.CanExecuteChanged += (s, e) => UpdateIsPrimaryButtonEnabled();
		NameInput.SelectAll();
	}

	private void HandlePrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args) =>
		ViewModel.SaveCommand.Execute(null);

	private void UpdateIsPrimaryButtonEnabled() =>
		IsPrimaryButtonEnabled = ViewModel.SaveCommand.CanExecute(null);
}
