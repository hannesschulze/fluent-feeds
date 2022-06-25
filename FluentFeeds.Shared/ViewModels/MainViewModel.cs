using System;
using System.Windows.Input;
using FluentFeeds.Shared.Models;
using FluentFeeds.Shared.Services;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;

namespace FluentFeeds.Shared.ViewModels;

/// <summary>
/// View model for the main pages managing navigation between feeds and other pages.
/// </summary>
public class MainViewModel : ObservableObject
{
	/// <summary>
	/// Direct child pages of this view model.
	/// </summary>
	public enum Page
	{
		/// <summary>
		/// App settings
		/// </summary>
		Settings,
		/// <summary>
		/// Feed viewer
		/// </summary>
		Feed
	}

	public MainViewModel(INavigationService navigationService)
	{
		_navigationService = navigationService;
		_navigationService.BackStackChanged += HandleBackStackChanged;

		_goBackCommand = new RelayCommand(() => _navigationService.GoBack(), () => _navigationService.CanGoBack);
		_visiblePage = GetVisiblePage();
	}

	/// <summary>
	/// Go back to the previous page/feed/article.
	/// </summary>
	public ICommand GoBackCommand => _goBackCommand;

	/// <summary>
	/// The currently visible child page.
	/// </summary>
	public Page VisiblePage
	{
		get => _visiblePage;
		private set => SetProperty(ref _visiblePage, value);
	}

	private void HandleBackStackChanged(object? sender, EventArgs e)
	{
		_goBackCommand.NotifyCanExecuteChanged();
		VisiblePage = GetVisiblePage();
	}

	private Page GetVisiblePage() =>
		_navigationService.CurrentEntry.Type switch
		{
			NavigationEntryType.Settings => Page.Settings,
			NavigationEntryType.FeedItem => Page.Feed,
			_ => throw new IndexOutOfRangeException()
		};

	private readonly INavigationService _navigationService;
	private readonly RelayCommand _goBackCommand;
	private Page _visiblePage;
}
