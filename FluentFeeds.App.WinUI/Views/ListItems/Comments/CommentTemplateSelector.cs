using System;
using FluentFeeds.App.Shared.ViewModels.ListItems.Comments;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace FluentFeeds.App.WinUI.Views.ListItems.Comments;

/// <summary>
/// Template selector for <see cref="CommentBaseViewModel"/>s.
/// </summary>
public sealed class CommentTemplateSelector : DataTemplateSelector
{
	/// <summary>
	/// Template used for the header item.
	/// </summary>
	public DataTemplate? HeaderTemplate { get; set; }

	/// <summary>
	/// Template used for comment items.
	/// </summary>
	public DataTemplate? CommentTemplate { get; set; }

	protected override DataTemplate? SelectTemplateCore(object item)
	{
		return
			((CommentBaseViewModel)item).Type switch
			{
				CommentBaseViewModelType.Comment => CommentTemplate,
				CommentBaseViewModelType.Header => HeaderTemplate,
				_ => throw new IndexOutOfRangeException()
			};
	}
}
