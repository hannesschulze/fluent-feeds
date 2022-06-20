using Microsoft.UI.Xaml;

namespace FluentFeeds.WinUI;

public sealed partial class MainWindow : Window
{
	public MainWindow()
	{
		InitializeComponent();
	}

	private void MyButton_Click(object sender, RoutedEventArgs e)
	{
		_myButton.Content = "Clicked";
	}
}
