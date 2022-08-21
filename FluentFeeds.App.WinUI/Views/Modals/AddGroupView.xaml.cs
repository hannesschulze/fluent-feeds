using FluentFeeds.App.Shared.ViewModels.Modals;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace FluentFeeds.App.WinUI.Views.Modals;

/// <summary>
/// Dialog presenting an <see cref="AddGroupViewModel"/>.
/// </summary>
public sealed partial class AddGroupView : ContentDialog
{
	public AddGroupView()
	{
		InitializeComponent();

		// XXX: ContentDialog's command handling seems buggy.
		Loading += HandleLoading;
	}

	public AddGroupViewModel ViewModel => (AddGroupViewModel)DataContext;

	private void HandleLoading(FrameworkElement sender, object args)
	{
		UpdateIsPrimaryButtonEnabled();
		ViewModel.ConfirmCommand.CanExecuteChanged += (s, e) => UpdateIsPrimaryButtonEnabled();
	}

	private void HandlePrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args) =>
		ViewModel.ConfirmCommand.Execute(null);

	private void UpdateIsPrimaryButtonEnabled() =>
		IsPrimaryButtonEnabled = ViewModel.ConfirmCommand.CanExecute(null);
}
