using FluentFeeds.App.Shared.Models.Navigation;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using FluentFeeds.App.Shared.ViewModels.Pages;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using Microsoft.UI.Xaml.Media;
using System;

namespace FluentFeeds.App.WinUI.Views.Pages;

/// <summary>
/// Page displaying an article.
/// </summary>
public sealed partial class ArticlePage : Page
{
	public ArticlePage()
	{
		DataContext = Ioc.Default.GetRequiredService<ArticleViewModel>();
		InitializeComponent();
	}

	public ArticleViewModel ViewModel => (ArticleViewModel)DataContext;

	protected override void OnNavigatedTo(NavigationEventArgs e)
	{
		base.OnNavigatedTo(e);
		MainScrollViewer.ScrollToVerticalOffset(0);
		ViewModel.Load((FeedNavigationRoute)e.Parameter);
	}

	private FontFamily GetFontFamily(Shared.Models.FontFamily fontFamily) =>
		fontFamily switch
		{
			Shared.Models.FontFamily.SansSerif => new("Segoe UI"),
			Shared.Models.FontFamily.Serif => new("Times New Roman"),
			Shared.Models.FontFamily.Monospace => new("Consolas"),
			_ => throw new IndexOutOfRangeException()
		};

	private double GetBodyFontSize(Shared.Models.FontSize fontSize) =>
		fontSize switch
		{
			Shared.Models.FontSize.Small => 12,
			Shared.Models.FontSize.Normal => 14,
			Shared.Models.FontSize.Large => 16,
			Shared.Models.FontSize.ExtraLarge => 20,
			_ => throw new IndexOutOfRangeException()
		};

	private double GetBodyLineHeight(Shared.Models.FontSize fontSize) =>
		fontSize switch
		{
			Shared.Models.FontSize.Small => 16,
			Shared.Models.FontSize.Normal => 20,
			Shared.Models.FontSize.Large => 22,
			Shared.Models.FontSize.ExtraLarge => 26,
			_ => throw new IndexOutOfRangeException()
		};

	private double GetHeading1FontSize(Shared.Models.FontSize fontSize) =>
		fontSize switch
		{
			Shared.Models.FontSize.Small => 24,
			Shared.Models.FontSize.Normal => 28,
			Shared.Models.FontSize.Large => 32,
			Shared.Models.FontSize.ExtraLarge => 36,
			_ => throw new IndexOutOfRangeException()
		};

	private double GetHeading1LineHeight(Shared.Models.FontSize fontSize) =>
		fontSize switch
		{
			Shared.Models.FontSize.Small => 32,
			Shared.Models.FontSize.Normal => 36,
			Shared.Models.FontSize.Large => 38,
			Shared.Models.FontSize.ExtraLarge => 42,
			_ => throw new IndexOutOfRangeException()
		};

	private double GetHeading2FontSize(Shared.Models.FontSize fontSize) =>
		fontSize switch
		{
			Shared.Models.FontSize.Small => 18,
			Shared.Models.FontSize.Normal => 21,
			Shared.Models.FontSize.Large => 24,
			Shared.Models.FontSize.ExtraLarge => 26,
			_ => throw new IndexOutOfRangeException()
		};

	private double GetHeading2LineHeight(Shared.Models.FontSize fontSize) =>
		fontSize switch
		{
			Shared.Models.FontSize.Small => 24,
			Shared.Models.FontSize.Normal => 28,
			Shared.Models.FontSize.Large => 28,
			Shared.Models.FontSize.ExtraLarge => 30,
			_ => throw new IndexOutOfRangeException()
		};

	private double GetHeading3FontSize(Shared.Models.FontSize fontSize) =>
		fontSize switch
		{
			Shared.Models.FontSize.Small => 14,
			Shared.Models.FontSize.Normal => 16,
			Shared.Models.FontSize.Large => 18,
			Shared.Models.FontSize.ExtraLarge => 22,
			_ => throw new IndexOutOfRangeException()
		};

	private double GetHeading3LineHeight(Shared.Models.FontSize fontSize) =>
		fontSize switch
		{
			Shared.Models.FontSize.Small => 18,
			Shared.Models.FontSize.Normal => 22,
			Shared.Models.FontSize.Large => 24,
			Shared.Models.FontSize.ExtraLarge => 28,
			_ => throw new IndexOutOfRangeException()
		};

	private double GetHeading4FontSize(Shared.Models.FontSize fontSize) =>
		fontSize switch
		{
			Shared.Models.FontSize.Small => 12,
			Shared.Models.FontSize.Normal => 14,
			Shared.Models.FontSize.Large => 16,
			Shared.Models.FontSize.ExtraLarge => 20,
			_ => throw new IndexOutOfRangeException()
		};

	private double GetHeading4LineHeight(Shared.Models.FontSize fontSize) =>
		fontSize switch
		{
			Shared.Models.FontSize.Small => 16,
			Shared.Models.FontSize.Normal => 20,
			Shared.Models.FontSize.Large => 22,
			Shared.Models.FontSize.ExtraLarge => 26,
			_ => throw new IndexOutOfRangeException()
		};

	private double GetHeading5FontSize(Shared.Models.FontSize fontSize) =>
		fontSize switch
		{
			Shared.Models.FontSize.Small => 10,
			Shared.Models.FontSize.Normal => 12,
			Shared.Models.FontSize.Large => 14,
			Shared.Models.FontSize.ExtraLarge => 16,
			_ => throw new IndexOutOfRangeException()
		};

	private double GetHeading5LineHeight(Shared.Models.FontSize fontSize) =>
		fontSize switch
		{
			Shared.Models.FontSize.Small => 12,
			Shared.Models.FontSize.Normal => 15,
			Shared.Models.FontSize.Large => 18,
			Shared.Models.FontSize.ExtraLarge => 20,
			_ => throw new IndexOutOfRangeException()
		};

	private double GetHeading6FontSize(Shared.Models.FontSize fontSize) =>
		fontSize switch
		{
			Shared.Models.FontSize.Small => 9,
			Shared.Models.FontSize.Normal => 10,
			Shared.Models.FontSize.Large => 12,
			Shared.Models.FontSize.ExtraLarge => 14,
			_ => throw new IndexOutOfRangeException()
		};

	private double GetHeading6LineHeight(Shared.Models.FontSize fontSize) =>
		fontSize switch
		{
			Shared.Models.FontSize.Small => 11,
			Shared.Models.FontSize.Normal => 12,
			Shared.Models.FontSize.Large => 16,
			Shared.Models.FontSize.ExtraLarge => 18,
			_ => throw new IndexOutOfRangeException()
		};
}
