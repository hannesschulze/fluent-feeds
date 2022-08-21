using FluentFeeds.App.Shared.ViewModels.Items.GroupSelection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace FluentFeeds.App.WinUI.Views.Items.GroupSelection;

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
