using System;
using System.Collections.Generic;
using System.Linq;

namespace FluentFeeds.Shared.RichText.Helpers;

internal static class CollectionHelper
{
	internal static int SequenceHashCode<TSource>(this IEnumerable<TSource> source) =>
		source
			.Aggregate(new HashCode(), (hash, item) =>
			{
				hash.Add(item);
				return hash;
			})
			.ToHashCode();

	internal static string SequenceString<TSource>(this IEnumerable<TSource> source) =>
		$"{{ {String.Join(", ", source.Select(item => item?.ToString() ?? "null"))} }}";
}
