using System;
using System.Collections.Immutable;
using System.Linq;
using FluentFeeds.Common.Helpers;

namespace FluentFeeds.Feeds.Base.Items.Content;

/// <summary>
/// <para>Content of an comment-based item.</para>
///
/// <para>This object stores a list of top-level comments for the item.</para>
/// </summary>
public sealed class CommentItemContent : ItemContent
{
	public CommentItemContent()
	{
		Comments = ImmutableArray<Comment>.Empty;
	}

	public CommentItemContent(params Comment[] comments)
	{
		Comments = ImmutableArray.Create(comments);
	}

	/// <summary>
	/// Top-level comments of the item.
	/// </summary>
	public ImmutableArray<Comment> Comments { get; init; }

	public override ItemContentType Type => ItemContentType.Comment;

	public override string ToString() => $"CommentItemContent {{ Comments = {Comments.SequenceString()} }}";

	public override int GetHashCode() => HashCode.Combine(base.GetHashCode(), Comments.SequenceHashCode());

	public override bool Equals(ItemContent? other)
	{
		if (ReferenceEquals(this, other))
			return true;
		if (!base.Equals(other))
			return false;
		return other is CommentItemContent casted && Comments.SequenceEqual(casted.Comments);
	}
}
