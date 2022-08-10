namespace FluentFeeds.Feeds.Base.Items.Content;

/// <summary>
/// The content of an item in a particular form.
/// </summary>
public abstract class ItemContent
{	
	/// <summary>
	/// Accept a visitor on this content.
	/// </summary>
	public abstract void Accept(IItemContentVisitor visitor);
	
	/// <summary>
	/// The content type.
	/// </summary>
	public abstract ItemContentType Type { get; }
}
