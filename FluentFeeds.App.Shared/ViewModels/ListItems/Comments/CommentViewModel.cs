using System;
using System.Collections.Generic;
using System.Linq;
using FluentFeeds.App.Shared.Models;
using FluentFeeds.App.Shared.Resources;
using FluentFeeds.Documents;
using FluentFeeds.Feeds.Base.Items.Content;

namespace FluentFeeds.App.Shared.ViewModels.ListItems.Comments;

/// <summary>
/// The main comment list item view model.
/// </summary>
public sealed class CommentViewModel : CommentBaseViewModel
{
	private static string FormatTimestamp(DateTimeOffset timestamp)
	{
		var localTimestamp = timestamp.LocalDateTime;
		return localTimestamp.ToString(localTimestamp.Date == DateTime.Today ? "t" : "d");
	}
	
	public CommentViewModel(Comment comment, FontFamily fontFamily, FontSize fontSize)
	{
		_fontFamily = fontFamily;
		_fontSize = fontSize;

		var timestamp = FormatTimestamp(comment.PublishedTimestamp);
		CommentInfo = comment.Author != null ? $"{comment.Author} | {timestamp}" : timestamp;
		CommentBody = comment.Body;
		Children = comment.Children.Select(c => new CommentViewModel(c, fontFamily, fontSize)).ToArray();
	}

	public override CommentBaseViewModelType Type => CommentBaseViewModelType.Comment;

	/// <summary>
	/// Line of text shown above the body.
	/// </summary>
	public string CommentInfo { get; }
	
	/// <summary>
	/// The comment content.
	/// </summary>
	public RichText CommentBody { get; }

	/// <summary>
	/// The font family used to display the comment's body.
	/// </summary>
	public FontFamily FontFamily
	{
		get => _fontFamily;
		set => SetProperty(ref _fontFamily, value);
	}

	/// <summary>
	/// The font size used to display the comment's body.
	/// </summary>
	public FontSize FontSize
	{
		get => _fontSize;
		set => SetProperty(ref _fontSize, value);
	}
	
	/// <summary>
	/// Child comments displayed below this one.
	/// </summary>
	public IReadOnlyList<CommentViewModel> Children { get; }

	private FontFamily _fontFamily;
	private FontSize _fontSize;
}
