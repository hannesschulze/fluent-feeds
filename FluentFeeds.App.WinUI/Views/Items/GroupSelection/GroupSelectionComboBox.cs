using FluentFeeds.App.Shared.ViewModels.Items.GroupSelection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace FluentFeeds.App.WinUI.Views.Items.GroupSelection;

/// <summary>
/// Custom combobox subclass updating the combobox item "IsEnabled" state according to the view model.
/// </summary>
public sealed class GroupSelectionComboBox : ComboBox
{
	protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
	{
		base.PrepareContainerForItemOverride(element, item);

		if (element is ComboBoxItem container && item is GroupSelectionItemViewModel viewModel)
		{
			container.IsEnabled = viewModel.IsSelectable;
		}
	}
}
