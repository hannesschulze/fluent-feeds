namespace FluentFeeds.Feeds.Base.Nodes;

/// <summary>
/// The type of a feed node.
/// </summary>
public enum FeedNodeType
{
	/// <summary>
	/// Group node, the node provides a composite feed combining all child feeds of the node.
	/// </summary>
	Group,
	/// <summary>
	/// Node with a custom feed provided by a <see cref="FeedProvider"/>. The feed provider must be able to serialize
	/// and deserialize the feed.
	/// </summary>
	Custom
}
