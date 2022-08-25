using FluentFeeds.App.Shared.ViewModels.Modals;
using Microsoft.UI.Xaml;

namespace FluentFeeds.App.WinUI.Views.Modals;

/// <summary>
/// Flyout content presenting a <see cref="DeleteFeedViewModel"/>.
/// </summary>
public sealed partial class DeleteFeedView : FlyoutView
{
	public DeleteFeedView()
	{
		InitializeComponent();
	}

	public DeleteFeedViewModel ViewModel => (DeleteFeedViewModel)DataContext;

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
