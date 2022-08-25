using FluentFeeds.App.Shared.ViewModels.Modals;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace FluentFeeds.App.WinUI.Views.Modals;

/// <summary>
/// Dialog presenting a <see cref="FeedDataViewModel"/>.
/// </summary>
public sealed partial class FeedDataView : ContentDialog
{
	public FeedDataView()
	{
		InitializeComponent();

		Loading += (s, e) => InputTextBox.SelectAll();
	}

	public FeedDataViewModel ViewModel => (FeedDataViewModel)DataContext;

	private async void HandlePrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
	{
		var deferral = args.GetDeferral();
		var success = await ViewModel.HandleSaveAsync();
		args.Cancel = !success;
		deferral.Complete();

		if (!success)
		{
			// ContentDialog loses focus after this event even though it is the only visible modal window.
			Focus(FocusState.Programmatic);
		}
	}

	private Thickness GetInfoBarMargin(bool isVisible) => new(0, 0, 0, bottom: isVisible ? 12 : 0);
}
