using FluentFeeds.App.Shared.ViewModels.Pages;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace FluentFeeds.App.Shared.ViewModels.ListItems.Comments;

/// <summary>
/// Base class for items in the <see cref="CommentItemViewModel"/>.
/// </summary>
public abstract class CommentBaseViewModel : ObservableObject
{
	/// <summary>
	/// The type of this list item.
	/// </summary>
	public abstract CommentBaseViewModelType Type { get; }
}
