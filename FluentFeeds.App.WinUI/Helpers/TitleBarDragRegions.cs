namespace FluentFeeds.App.WinUI.Helpers;

/// <summary>
/// Structure defining the draggable regions of a titlebar.
/// </summary>
public readonly record struct TitleBarDragRegions(
	double LeftStart, double LeftWidth, double RightStart, double RightWidth);
