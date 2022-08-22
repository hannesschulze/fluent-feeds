using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace FluentFeeds.App.Shared.Helpers;

/// <summary>
/// Helper class which transforms items from a source collection and updates a target collection.
/// </summary>
public sealed class ObservableCollectionTransformer<TSource, TTarget> where TSource : class where TTarget : class
{
	public ObservableCollectionTransformer(
		IReadOnlyList<TSource> sourceList, ObservableCollection<TTarget> targetList, 
		Func<TSource, TTarget> transformFunction)
	{
		SourceList = sourceList;
		TargetList = targetList;
		TargetOffset = TargetList.Count;
		
		_transformFunction = transformFunction;

		for (var i = 0; i < SourceList.Count; ++i)
		{
			TargetList.Add(GetTransformedItem(i));
		}
		_itemCount = SourceList.Count;

		if (SourceList is INotifyCollectionChanged observable)
		{
			observable.CollectionChanged += HandleSourceListChanged;
		}
	}
	
	/// <summary>
	/// The original list.
	/// </summary>
	public IReadOnlyList<TSource> SourceList { get; }

	/// <summary>
	/// The target list which is modified by this class.
	/// </summary>
	public ObservableCollection<TTarget> TargetList { get; }
	
	/// <summary>
	/// Offset added to indices in the target list. This is initially set to the target list's count.
	/// </summary>
	public int TargetOffset { get; set; }
	
	private void HandleAdd(int start, int count)
    {
    	for (var i = start; i < start + count; ++i)
    	{
    		TargetList.Insert(i + TargetOffset, GetTransformedItem(i));
    	}
    }

    private void HandleRemove(int start, int count)
    {
    	for (var i = 0; i < count; ++i)
    	{
    		TargetList.RemoveAt(start + TargetOffset);
    	}
    }

    private void HandleReplace(int start, int oldCount, int newCount)
    {
    	if (oldCount == 1 && newCount == 1)
        {
	        TargetList[start + TargetOffset] = GetTransformedItem(start);
        }
    	else
    	{
    		HandleRemove(start, oldCount);
    		HandleAdd(start, newCount);
    	}
    }

    private void HandleMove(int oldStart, int newStart, int count)
    {
    	if (count == 1)
    	{
	        TargetList.Move(oldStart + TargetOffset, newStart + TargetOffset);
    	}
    	else
    	{
    		HandleRemove(oldStart, count);
    		HandleAdd(newStart, count);
    	}
    }

    private void HandleReset()
    {
	    HandleRemove(0, _itemCount);
	    HandleAdd(0, SourceList.Count);
    }

    private void HandleSourceListChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
    	switch (e.Action)
    	{
    		case NotifyCollectionChangedAction.Add:
    			HandleAdd(e.NewStartingIndex, e.NewItems?.Count ?? 0);
    			break;
    		case NotifyCollectionChangedAction.Remove:
    			HandleRemove(e.OldStartingIndex, e.OldItems?.Count ?? 0);
    			break;
    		case NotifyCollectionChangedAction.Replace:
    			HandleReplace(e.NewStartingIndex, e.OldItems?.Count ?? 0, e.NewItems?.Count ?? 0);
    			break;
    		case NotifyCollectionChangedAction.Move:
    			HandleMove(e.OldStartingIndex, e.NewStartingIndex, e.OldItems?.Count ?? 0);
    			break;
    		case NotifyCollectionChangedAction.Reset:
    			HandleReset();
    			break;
    		default:
    			throw new IndexOutOfRangeException();
    	}

        _itemCount = SourceList.Count;
    }

    private TTarget GetTransformedItem(int index) => _transformFunction.Invoke(SourceList[index]);

	private readonly Func<TSource, TTarget> _transformFunction;
	private int _itemCount;
}

public static class ObservableCollectionTransformer
{
	public static ObservableCollectionTransformer<TSource, TTarget> CreateCached<TSource, TTarget>(
		IReadOnlyList<TSource> sourceList, ObservableCollection<TTarget> targetList,
		Func<TSource, TTarget> transformFunction, Dictionary<TSource, TTarget>? cache = null)
		where TSource : class
		where TTarget : class
	{
		return CreateCached(sourceList, targetList, transformFunction, source => source, cache);
	}

	public static ObservableCollectionTransformer<TSource, TTarget> CreateCached<TSource, TTarget, TCacheKey>(
		IReadOnlyList<TSource> sourceList, ObservableCollection<TTarget> targetList,
		Func<TSource, TTarget> transformFunction, Func<TSource, TCacheKey> cacheKeyFunction,
		Dictionary<TCacheKey, TTarget>? cache = null)
		where TSource : class
		where TTarget : class
		where TCacheKey : notnull
	{
		cache ??= new Dictionary<TCacheKey, TTarget>();
		return new ObservableCollectionTransformer<TSource, TTarget>(
			sourceList, targetList, source =>
			{
				var cacheKey = cacheKeyFunction.Invoke(source);
				if (cache.TryGetValue(cacheKey, out var existing))
					return existing;
				
				var target = transformFunction.Invoke(source);
				cache.Add(cacheKey, target);
				return target;
			});
	}
}
