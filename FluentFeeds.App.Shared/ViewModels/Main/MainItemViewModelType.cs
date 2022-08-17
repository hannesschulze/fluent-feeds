namespace FluentFeeds.App.Shared.ViewModels.Main;

/// <summary>
/// The type of a list item on the main page.
/// </summary>
public enum MainItemViewModelType
{
	/// <summary>
	/// <see cref="MainLoadingItemViewModel"/>
	/// </summary>
	Loading,
	/// <summary>
	/// <see cref="MainNavigationItemViewModel"/>
	/// </summary>
	Navigation
}
