using FluentFeeds.App.Shared.ViewModels.Pages;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using Microsoft.UI.Xaml.Controls;

namespace FluentFeeds.App.WinUI.Views.Pages;

/// <summary>
/// Page displaying a content item.
/// </summary>
public sealed partial class CommentItemPage : Page
{
	public CommentItemPage()
	{
		DataContext = Ioc.Default.GetRequiredService<CommentItemViewModel>();
		InitializeComponent();
	}

	public CommentItemViewModel ViewModel => (CommentItemViewModel)DataContext;
}
