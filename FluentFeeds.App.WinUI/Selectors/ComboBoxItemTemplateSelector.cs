using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace FluentFeeds.App.WinUI.Selectors;

/// <summary>
/// A data template selector which picks the template based on the item type in a context menu.
/// </summary>
/// <remarks>
/// This selector is based on: https://stackoverflow.com/a/3996627
/// </remarks>
public sealed class ComboBoxItemTemplateSelector : DataTemplateSelector
{
	/// <summary>
	/// Template used in the context menu.
	/// </summary>
	public DataTemplate? MenuTemplate { get; set; }

	/// <summary>
	/// Template used for the selected item.
	/// </summary>
	public DataTemplate? SelectedTemplate { get; set; }

	protected override DataTemplate? SelectTemplateCore(object item, DependencyObject container)
	{
		while (container != null)
		{
			if (container is ComboBoxItem)
			{
				return MenuTemplate;
			}
			container = VisualTreeHelper.GetParent(container);
		}

		return SelectedTemplate;
	}
}
