using System;

namespace FluentFeeds.App.Shared.ViewModels.ListItems.Comments;

/// <summary>
/// A header item which is displayed at the top of the list and presents the item's title and info area.
/// </summary>
public sealed class CommentHeaderViewModel : CommentBaseViewModel
{
	public override CommentBaseViewModelType Type => CommentBaseViewModelType.Header;

	/// <summary>
	/// Title of the item.
	/// </summary>
	public string Title
	{
		get => _title;
		set => SetProperty(ref _title, value);
	}

	/// <summary>
	/// Line of text shown below the title.
	/// </summary>
	public string ItemInfo
	{
		get => _itemInfo;
		set => SetProperty(ref _itemInfo, value);
	}

	private string _title = String.Empty;
	private string _itemInfo = String.Empty;
}
