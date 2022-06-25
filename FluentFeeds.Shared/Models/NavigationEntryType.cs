using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentFeeds.Shared.Models;

/// <summary>
/// Type of a <see cref="NavigationEntry"/>.
/// </summary>
public enum NavigationEntryType
{
	/// <summary>
	/// App settings.
	/// </summary>
	Settings,
	/// <summary>
	/// An optional item in a feed.
	/// </summary>
	FeedItem
}
