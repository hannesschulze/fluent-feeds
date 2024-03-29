using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using FluentFeeds.App.Shared.Models.Feeds.Loaders;
using FluentFeeds.App.Shared.Models.Storage;
using FluentFeeds.App.Shared.Resources;
using FluentFeeds.Common;
using FluentFeeds.Feeds.Base.Feeds.Content;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace FluentFeeds.App.Shared.Models.Feeds;

/// <summary>
/// Stored representation of a feed.
/// </summary>
public class Feed : ObservableObject, IFeedView
{
	public Feed(
		Guid identifier, IFeedStorage? storage, Func<IFeedView, FeedLoader> loaderFactory, bool hasChildren,
		IFeedView? parent, string? name, Symbol? symbol, FeedMetadata metadata, bool isUserCustomizable,
		bool isExcludedFromGroup)
	{
		Identifier = identifier;
		Storage = storage;
		_loader = new Lazy<FeedLoader>(
			() =>
			{
				var loader = loaderFactory.Invoke(this);
				loader.MetadataUpdater = UpdateMetadataAsync;
				return loader;
			});
		if (hasChildren)
		{
			Children = new ObservableCollection<IFeedView>();
			_readOnlyChildren = new ReadOnlyObservableCollection<IFeedView>(Children);
		}
		_parent = parent;
		_name = name;
		_symbol = symbol;
		_metadata = metadata;
		_displayName = GetDisplayName();
		_displaySymbol = GetDisplaySymbol();
		_isUserCustomizable = isUserCustomizable;
		_isExcludedFromGroup = isExcludedFromGroup;
	}

	public Guid Identifier { get; }
	
	public IFeedStorage? Storage { get; }

	public FeedLoader Loader => _loader.Value;

	public ObservableCollection<IFeedView>? Children { get; }

	ReadOnlyObservableCollection<IFeedView>? IFeedView.Children => _readOnlyChildren;

	public IFeedView? Parent
	{
		get => _parent;
		set => SetProperty(ref _parent, value);
	}

	public string? Name
	{
		get => _name;
		set
		{
			if (SetProperty(ref _name, value))
			{
				DisplayName = GetDisplayName();
			}
		}
	}

	public Symbol? Symbol
	{
		get => _symbol;
		set
		{
			if (SetProperty(ref _symbol, value))
			{
				DisplaySymbol = GetDisplaySymbol();
			}
		}
	}

	public FeedMetadata Metadata
	{
		get => _metadata;
		set
		{
			if (SetProperty(ref _metadata, value))
			{
				DisplayName = GetDisplayName();
				DisplaySymbol = GetDisplaySymbol();
			}
		}
	}

	public string DisplayName
	{
		get => _displayName;
		private set => SetProperty(ref _displayName, value);
	}

	public Symbol DisplaySymbol
	{
		get => _displaySymbol;
		private set => SetProperty(ref _displaySymbol, value);
	}

	public bool IsUserCustomizable
	{
		get => _isUserCustomizable;
		set => SetProperty(ref _isUserCustomizable, value);
	}

	public bool IsExcludedFromGroup
	{
		get => _isExcludedFromGroup;
		set => SetProperty(ref _isExcludedFromGroup, value);
	}
	
	private string GetDisplayName() => Name ?? Metadata.Name ?? LocalizedStrings.FallbackFeedName;

	private Symbol GetDisplaySymbol() => Symbol ?? Metadata.Symbol ?? Common.Symbol.Feed;

	private Task UpdateMetadataAsync(FeedMetadata metadata)
	{
		if (metadata != Metadata && Storage != null)
		{
			return Storage.UpdateFeedMetadataAsync(Identifier, metadata);
		}
		
		return Task.CompletedTask;
	}

	private readonly Lazy<FeedLoader> _loader;
	private readonly ReadOnlyObservableCollection<IFeedView>? _readOnlyChildren;
	private IFeedView? _parent;
	private string? _name;
	private Symbol? _symbol;
	private FeedMetadata _metadata;
	private string _displayName;
	private Symbol _displaySymbol;
	private bool _isUserCustomizable;
	private bool _isExcludedFromGroup;
}
