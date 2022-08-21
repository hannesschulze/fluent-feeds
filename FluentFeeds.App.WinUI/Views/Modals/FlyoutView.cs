using System;
using Microsoft.UI.Xaml.Controls;

namespace FluentFeeds.App.WinUI.Views.Modals;

/// <summary>
/// Flyout currently cannot be used from XAML. This is a base class for the flyout's content which exposes
/// the parent flyout.
/// </summary>
public abstract class FlyoutView : UserControl
{
	/// <summary>
	/// Create a flyout containing this view.
	/// </summary>
	public Flyout CreateFlyout()
	{
		if (Flyout != null)
			throw new Exception("FlyoutViews can only be used in a single flyout.");
		Flyout = new Flyout();
		Flyout.Content = this;
		ConfigureFlyout(Flyout);
		return Flyout;
	}

	protected virtual void ConfigureFlyout(Flyout flyout)
	{
	}

	/// <summary>
	/// The attached flyout.
	/// </summary>
	public Flyout? Flyout { get; private set; }
}
