namespace FluentFeeds.App.Shared.ViewModels.Main;

/// <summary>
/// List item on the main page indicating that the list of feeds is currently still loading.  
/// </summary>
public sealed class MainLoadingItemViewModel : MainItemViewModel
{
	public MainLoadingItemViewModel() : base(isSelectable: false, isExpandable: false)
	{
	}

	public override MainItemViewModelType Type => MainItemViewModelType.Loading;
}
