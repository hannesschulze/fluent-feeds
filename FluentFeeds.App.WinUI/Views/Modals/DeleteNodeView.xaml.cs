using FluentFeeds.App.Shared.ViewModels.Modals;
using Microsoft.UI.Xaml;

namespace FluentFeeds.App.WinUI.Views.Modals;

/// <summary>
/// Flyout content presenting a <see cref="DeleteNodeViewModel"/>.
/// </summary>
public sealed partial class DeleteNodeView : FlyoutView
{
	public DeleteNodeView()
	{
		InitializeComponent();
	}

	public DeleteNodeViewModel ViewModel => (DeleteNodeViewModel)DataContext;

	private void HandleConfirmClicked(object sender, RoutedEventArgs e)
	{
		ViewModel.ConfirmCommand.Execute(null);
		Flyout?.Hide();
	}

	private void HandleCancelClicked(object sender, RoutedEventArgs e)
	{
		Flyout?.Hide();
	}
}
