using System;
using System.Collections.Generic;
using System.Linq;

namespace FluentFeeds.Common.Helpers;

public static class CollectionHelper
{
	public static int SequenceHashCode<TSource>(this IEnumerable<TSource> source) =>
		source
			.Aggregate(new HashCode(), (hash, item) =>
			{
				hash.Add(item);
				return hash;
			})
			.ToHashCode();

	public static string SequenceString<TSource>(this IReadOnlyCollection<TSource> source) =>
		$"[{source.Count} {(source.Count == 1 ? "element" : "elements")}]";
}
