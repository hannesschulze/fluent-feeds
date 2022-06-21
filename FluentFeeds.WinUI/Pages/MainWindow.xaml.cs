using FluentFeeds.Shared.ViewModels;
using Microsoft.UI.Xaml;

namespace FluentFeeds.WinUI.Pages;

public sealed partial class MainWindow : Window
{
	public MainWindow(MainViewModel viewModel)
	{
		ViewModel = viewModel;
		InitializeComponent();
	}

	public MainViewModel ViewModel { get; }
}
