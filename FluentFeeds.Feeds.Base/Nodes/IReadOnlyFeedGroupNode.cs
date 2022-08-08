using System.Collections.ObjectModel;

namespace FluentFeeds.Feeds.Base.Nodes;

/// <summary>
/// Read-only view into a group node.
/// </summary>
public interface IReadOnlyFeedGroupNode : IReadOnlyFeedNode
{
	/// <summary>
	/// Child nodes of this node.
	/// </summary>
	ReadOnlyObservableCollection<IReadOnlyFeedNode> Children { get; }
	
	/// <summary>
	/// Flag indicating if the user can customize this group (i.e. rename it or add and remove items). 
	/// </summary>
	bool IsUserCustomizable { get; }
}
