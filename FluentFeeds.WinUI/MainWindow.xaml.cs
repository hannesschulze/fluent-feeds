using FluentFeeds.WinUI.Helpers;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;

namespace FluentFeeds.WinUI;

public sealed partial class MainWindow : Window
{
	public MainWindow()
	{
		InitializeComponent();

		Title = _mainPage.WindowTitle;
		this.GetAppWindow().SetIcon(_mainPage.WindowIcon);

		if (MicaController.IsSupported())
		{
			_micaHelper = new MicaHelper(this);
		}

		if (AppWindowTitleBar.IsCustomizationSupported())
		{
			_titleBarHelper = new TitleBarHelper(this, () => _mainPage.ComputeTitleBarDragRegions());
			_mainPage.CaptionButtonsWidth = _titleBarHelper.CaptionButtonsWidth;
			_mainPage.TitleBarHeight = _titleBarHelper.TitleBarHeight;
			_mainPage.DragRegionSizeChanged += (s, e) => _titleBarHelper.UpdateDragRegions();
		}
	}

	private MicaHelper? _micaHelper;
	private TitleBarHelper? _titleBarHelper;
}
