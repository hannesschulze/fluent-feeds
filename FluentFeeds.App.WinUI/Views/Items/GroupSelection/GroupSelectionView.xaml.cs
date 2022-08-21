using FluentFeeds.App.Shared.ViewModels.Items.GroupSelection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace FluentFeeds.App.WinUI.Views.Items.GroupSelection;

/// <summary>
/// View displaying a list of <see cref="GroupSelectionItemViewModel"/>s.
/// </summary>
public sealed partial class GroupSelectionView : UserControl
{
	public GroupSelectionView()
	{
		InitializeComponent();
	}

	public GroupSelectionViewModel ViewModel => (GroupSelectionViewModel)DataContext;
}
