using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using FluentFeeds.App.Shared.Models.Navigation;
using FluentFeeds.App.Shared.Services;
using FluentFeeds.App.Shared.ViewModels.ListItems.Comments;

namespace FluentFeeds.App.Shared.ViewModels.Pages;

public sealed class CommentItemViewModel : ItemViewModel
{
	public CommentItemViewModel(ISettingsService settingsService)
	{
		_settingsService = settingsService;
		_settingsService.PropertyChanged += HandleSettingsChanged;

		_items = new List<CommentBaseViewModel> { _header };
	}

	public override void Load(FeedNavigationRoute route)
	{
		if (route.Type != FeedNavigationRouteType.CommentItem)
			throw new Exception("Invalid route type.");

		base.Load(route);
		var content = route.CommentContent!;
		_comments = content.Comments
			.Select(comment => new CommentViewModel(
				comment, _settingsService.ContentFontFamily, _settingsService.ContentFontSize))
			.ToArray();

		var items = new List<CommentBaseViewModel> { Capacity = _comments.Count + 1 };
		items.Add(_header);
		items.AddRange(_comments);
		Items = items;
	}

	/// <summary>
	/// The comment items displayed in a tree.
	/// </summary>
	public IReadOnlyList<CommentBaseViewModel> Items
	{
		get => _items;
		private set => SetProperty(ref _items, value);
	}

	protected override void UpdateTitle(string title)
	{
		_header.Title = title;
	}

	protected override void UpdateItemInfo(string itemInfo)
	{
		_header.ItemInfo = itemInfo;
	}

	private void UpdateCommentViewModels(Action<CommentViewModel> action)
	{
		if (_comments != null)
		{
			var stack = new Stack<CommentViewModel>();
			foreach (var comment in _comments)
			{
				stack.Push(comment);
			}
			while (stack.TryPop(out var comment))
			{
				action.Invoke(comment);

				foreach (var child in comment.Children)
				{
					stack.Push(child);
				}
			}
		}
	}

	private void HandleSettingsChanged(object? sender, PropertyChangedEventArgs e)
	{
		switch (e.PropertyName)
		{
			case nameof(ISettingsService.ContentFontFamily):
				UpdateCommentViewModels(vm => vm.FontFamily = _settingsService.ContentFontFamily);
				break;
			case nameof(ISettingsService.ContentFontSize):
				UpdateCommentViewModels(vm => vm.FontSize = _settingsService.ContentFontSize);
				break;
		}
	}

	private readonly ISettingsService _settingsService;
	private readonly CommentHeaderViewModel _header = new();
	private IReadOnlyList<CommentViewModel>? _comments;
	private IReadOnlyList<CommentBaseViewModel> _items;
}
