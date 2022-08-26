using System;
using System.Collections.Immutable;
using System.Linq;
using FluentFeeds.Common.Helpers;
using FluentFeeds.Documents;

namespace FluentFeeds.Feeds.Base.Items.Content;

/// <summary>
/// A comment which is part of a <see cref="CommentItemContent"/> object.
/// </summary>
public sealed class Comment : IEquatable<Comment>
{
	public Comment()
	{
		PublishedTimestamp = DateTimeOffset.Now;
		Body = new RichText();
		Children = ImmutableArray<Comment>.Empty;
	}

	public Comment(string? author, DateTimeOffset publishedTimestamp, RichText body, params Comment[] children)
	{
		Author = author;
		PublishedTimestamp = publishedTimestamp;
		Body = body;
		Children = ImmutableArray.Create(children);
	}

	/// <summary>
	/// The author who posted the comment.
	/// </summary>
	public string? Author { get; init; }

	/// <summary>
	/// Timestamp at which the comment was posted.
	/// </summary>
	public DateTimeOffset PublishedTimestamp { get; init; }

	/// <summary>
	/// The comment's body.
	/// </summary>
	public RichText Body { get; init; }

	/// <summary>
	/// Child comments for this comment.
	/// </summary>
	public ImmutableArray<Comment> Children { get; init; }

	public bool Equals(Comment? other)
	{
		if (ReferenceEquals(this, other))
			return true;
		if (ReferenceEquals(null, other))
			return false;
		return
			Author == other.Author && PublishedTimestamp == other.PublishedTimestamp && Body == other.Body &&
			Children.SequenceEqual(other.Children);
	}

	public override bool Equals(object? obj)
	{
		if (ReferenceEquals(this, obj))
			return true;
		return obj is Comment other && Equals(other);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(Author, PublishedTimestamp, Body, Children.SequenceHashCode());
	}

	public override string ToString() =>
		$"Comment {{ Author = {Author}, PublishedTimestamp = {PublishedTimestamp}, Body = {Body}, " +
		$"Children = {Children.SequenceString()} }}";

	public static bool operator ==(Comment? lhs, Comment? rhs) => lhs?.Equals(rhs) ?? ReferenceEquals(rhs, null);

	public static bool operator !=(Comment? lhs, Comment? rhs) => !(lhs == rhs);
}
