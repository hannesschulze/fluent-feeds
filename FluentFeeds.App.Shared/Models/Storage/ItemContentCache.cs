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
	private const int MaxCacheSize = 10;
	
	public ItemContentCache(IDatabaseService databaseService)
	{
		_databaseService = databaseService;
	}
	
	public async Task<IItemContentLoader> GetLoaderAsync(Guid itemIdentifier, FeedProvider provider)
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

		var newTask = LoadFromDatabaseAsync(itemIdentifier, provider);
		_loading.Add(itemIdentifier, newTask);
		var result = await newTask;
		_loading.Remove(itemIdentifier);
		if (_cacheOrder.Count >= MaxCacheSize)
		{
			_cached.Remove(_cacheOrder[0]);
			_cacheOrder.RemoveAt(0);
		}
		_cached.Add(itemIdentifier, result);
		_cacheOrder.Add(itemIdentifier);
		return result;
	}

	private Task<IItemContentLoader> LoadFromDatabaseAsync(Guid itemIdentifier, FeedProvider provider)
	{
		return _databaseService.ExecuteAsync(
			async database =>
			{
				var serialized = await database.Items
					.Where(i => i.Identifier == itemIdentifier)
					.Select(i => i.Content)
					.FirstAsync();
				return await provider.LoadItemContentAsync(serialized);
			});
	}

	private readonly IDatabaseService _databaseService;
	private readonly List<Guid> _cacheOrder = new();
	private readonly Dictionary<Guid, IItemContentLoader> _cached = new();
	private readonly Dictionary<Guid, Task<IItemContentLoader>> _loading = new();
}
