using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using FluentFeeds.Shared.Models;

namespace FluentFeeds.Shared.ViewModels;

public class ItemListViewModel
{
	protected void UpdateItems(IEnumerable<Item> newItems, bool computeDifference = true)
	{
		var newUniqueItems = newItems.Distinct().ToList();
		if (computeDifference)
		{
			var oldItemsSet = _items.ToHashSet();
			var newItemsSet = newUniqueItems.ToHashSet();

			// Remove old items
			for (var i = _items.Count - 1; i >= 0; --i)
			{
				var item = _items[i];
				if (!newItemsSet.Contains(item))
				{
					_items.RemoveAt(i);
				}
			}
			Debug.Assert(_items.Count <= newUniqueItems.Count);

			// Add new items and find out which items were moved.
			var movedItems = new Dictionary<Item, int>();
			for (var i = 0; i < newUniqueItems.Count; ++i)
			{
				var item = newUniqueItems[i];
				if (!oldItemsSet.Contains(item))
				{
					_items.Insert(i, item);
				}
				else if (_items[i] != item)
				{
					movedItems.Add(item, i);
				}
			}
			Debug.Assert(_items.Count == newUniqueItems.Count);
			
			// Find source indices for moved items.
			if (movedItems.Count != 0)
			{
				for (var i = 0; i < _items.Count; ++i)
				{
					var item = _items[i];
					if (movedItems.TryGetValue(item, out var newIndex))
					{
						_items.Move(i, newIndex);
					}
				}
			}
			Debug.Assert(_items.SequenceEqual(newUniqueItems));
		}
		else
		{
			_items.Clear();
			foreach (var item in newUniqueItems)
			{
				_items.Add(item);
			}
		}
	}

	private readonly ObservableCollection<Item> _items = new();
}
