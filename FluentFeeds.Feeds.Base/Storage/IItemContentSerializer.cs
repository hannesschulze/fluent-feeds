using System.Threading.Tasks;
using FluentFeeds.Feeds.Base.Items.ContentLoaders;

namespace FluentFeeds.Feeds.Base.Storage;

/// <summary>
/// An interface for serializing and deserializing item content loaders.
/// </summary>
public interface IItemContentSerializer
{
	/// <summary>
	/// Serialize a content loader into a string.
	/// </summary>
	Task<string> StoreAsync(IItemContentLoader contentLoader);
	
	/// <summary>
	/// Deserialize a content loader from a string.
	/// </summary>
	Task<IItemContentLoader> LoadAsync(string serialized);
}
