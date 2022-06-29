using System;
using System.Collections.Generic;
using System.Linq;

namespace FluentFeeds.Shared.RichText.Helpers;

internal static class HashCodeHelper
{
	internal static int SequenceHashCode<TSource>(this IEnumerable<TSource> source) =>
		source
			.Aggregate(new HashCode(), (hash, item) =>
			{
				hash.Add(item);
				return hash;
			})
			.ToHashCode();
}
