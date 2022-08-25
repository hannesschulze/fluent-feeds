using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentFeeds.App.Shared.Services;
using FluentFeeds.Feeds.Base;
using FluentFeeds.Feeds.Base.Items.Content;
using Microsoft.EntityFrameworkCore;

namespace FluentFeeds.App.Shared.Models.Storage;

/// <summary>
/// A cache which can store a limited number of <see cref="IItemContentLoader"/> objects loaded from the database.
/// </summary>
public sealed class ItemContentCache
{
	public ItemContentCache(int maxCacheSize)
	{
		_maxCacheSize = maxCacheSize;
	}
	
	public async Task<IItemContentLoader> GetLoaderAsync(Guid itemIdentifier, Func<Task<IItemContentLoader>> loadFunc)
	{
		if (_cached.TryGetValue(itemIdentifier, out var cached))
		{
			_cacheOrder.Remove(itemIdentifier);
			_cacheOrder.Add(itemIdentifier);
			return cached;
		}
		
		if (_loading.TryGetValue(itemIdentifier, out var task))
		{
			return await task;
		}
		
		IItemContentLoader result;
		try
		{
			var newTask = loadFunc.Invoke();
			_loading.Add(itemIdentifier, newTask);
			result = await newTask;
		}
		catch (Exception)
		{
			_loading.Remove(itemIdentifier);
			throw;
		}
		_loading.Remove(itemIdentifier);
		if (_cacheOrder.Count >= _maxCacheSize)
		{
			_cached.Remove(_cacheOrder[0]);
			_cacheOrder.RemoveAt(0);
		}
		_cached.Add(itemIdentifier, result);
		_cacheOrder.Add(itemIdentifier);
		return result;
	}

	private readonly int _maxCacheSize;
	private readonly List<Guid> _cacheOrder = new();
	private readonly Dictionary<Guid, IItemContentLoader> _cached = new();
	private readonly Dictionary<Guid, Task<IItemContentLoader>> _loading = new();
}
