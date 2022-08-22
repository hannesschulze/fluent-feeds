using FluentFeeds.App.Shared.ViewModels.Pages;
using FluentFeeds.App.WinUI.Helpers;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using Microsoft.UI.Xaml.Controls;

namespace FluentFeeds.App.WinUI.Views.Pages;

public sealed partial class FeedPage : Page
{
	public FeedPage()
	{
		DataContext = Ioc.Default.GetRequiredService<FeedViewModel>();
		InitializeComponent();

		SynchronizeSymbol = Common.Symbol.Synchronize.ToIconElement();
		RefreshSymbol = Common.Symbol.Refresh.ToIconElement();
		MailUnreadSymbol = Common.Symbol.MailUnread.ToIconElement();
		TrashSymbol = Common.Symbol.Trash.ToIconElement();
		SortOrderSymbol = Common.Symbol.SortOrder.ToIconElement();
		OpenExternalSymbol = Common.Symbol.OpenExternal.ToIconElement();
		FontSymbol = Common.Symbol.Font.ToIconElement();
	}

	public FeedViewModel ViewModel => (FeedViewModel)DataContext;

	private IconElement SynchronizeSymbol { get; }
	private IconElement RefreshSymbol { get; }
	private IconElement MailUnreadSymbol { get; }
	private IconElement TrashSymbol { get; }
	private IconElement SortOrderSymbol { get; }
	private IconElement OpenExternalSymbol { get; }
	private IconElement FontSymbol { get; }
}
