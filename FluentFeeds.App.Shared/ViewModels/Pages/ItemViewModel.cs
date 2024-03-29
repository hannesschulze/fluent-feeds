﻿using System.ComponentModel;
using System;
using FluentFeeds.App.Shared.Models.Items;
using FluentFeeds.App.Shared.Models.Navigation;
using FluentFeeds.App.Shared.Resources;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace FluentFeeds.App.Shared.ViewModels.Pages;

/// <summary>
/// Base view model for pages displaying item content.
/// </summary>
public abstract class ItemViewModel : ObservableObject
{
	/// <summary>
	/// Called after navigating to the itempage.
	/// </summary>
	/// <param name="route">Route containing parameters.</param>
	public virtual void Load(FeedNavigationRoute route)
	{
		if (_item != null)
		{
			_item.PropertyChanged -= HandleItemPropertyChanged;
		}

		_item = route.Item;
		if (_item != null)
		{
			_item.PropertyChanged += HandleItemPropertyChanged;
		}

		UpdateTitle();
		UpdateItemInfo();
	}

	protected abstract void UpdateTitle(string title);

	protected abstract void UpdateItemInfo(string itemInfo);

	private void UpdateTitle()
	{
		if (_item != null)
		{
			UpdateTitle(_item?.Title ?? String.Empty);
		}
	}

	private void UpdateItemInfo()
	{
		if (_item != null)
		{
			var publishedTimestamp = _item.PublishedTimestamp.ToLocalTime();
			UpdateItemInfo(_item.Author != null
				? String.Format(LocalizedStrings.ItemInfoWithAuthor, _item.Author, publishedTimestamp.ToString("f"))
				: String.Format(LocalizedStrings.ItemInfoWithoutAuthor, publishedTimestamp.ToString("f")));
		}
	}

	private void HandleItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		switch (e.PropertyName)
		{
			case nameof(IItemView.Title):
				UpdateTitle();
				break;
			case nameof(IItemView.Author):
			case nameof(IItemView.PublishedTimestamp):
				UpdateItemInfo();
				break;
		}
	}

	private IItemView? _item;
	private string _title = String.Empty;
	private string _itemInfo = String.Empty;
}
