using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentFeeds.App.Shared.Models.Database;
using FluentFeeds.Feeds.Base;
using FluentFeeds.Feeds.Base.Nodes;
using FluentFeeds.Feeds.Base.Storage;
using Microsoft.EntityFrameworkCore;

namespace FluentFeeds.App.Shared.Services.Default;

public partial class FeedService
{
	private sealed class FeedStorage : IFeedStorage
	{
		public FeedStorage(IDatabaseService databaseService, FeedProvider provider)
		{
			_databaseService = databaseService;
			Provider = provider;
			Identifier = Provider.Metadata.Identifier;
		}
		
		public FeedProvider Provider { get; }

		public Guid Identifier { get; }
		
		/// <summary>
		/// Load a feed node and all of its children from its database representation.
		/// </summary>
		private async Task<LoadedFeedNode> LoadNodeAsync(
			AppDbContext database, FeedNodeDb dbNode, LoadedFeedNode? parent,
			ICollection<LoadedFeedNode>? allNodes = null)
		{
			var node = new LoadedFeedNode(
				dbNode.Type switch
				{
					FeedNodeType.Group => FeedNode.Group(dbNode.Title, dbNode.Symbol, dbNode.IsUserCustomizable),
					FeedNodeType.Custom => FeedNode.Custom(
						await Provider.LoadFeedAsync(this, dbNode.Feed ?? String.Empty), dbNode.Title, dbNode.Symbol,
						dbNode.IsUserCustomizable, dbNode.HasChildren ? Enumerable.Empty<IReadOnlyFeedNode>() : null),
					_ => throw new IndexOutOfRangeException()
				}, this, dbNode, parent);
			allNodes?.Add(node);

			if (node.Children != null)
			{
				var dbChildren = await database.FeedNodes.Where(n => n.Parent == dbNode).ToListAsync();
				var children = new List<LoadedFeedNode>();
				foreach (var child in dbChildren)
				{
					children.Add(await LoadNodeAsync(database, child, node, allNodes));
				}

				var sortedChildren = children.OrderBy(n => n.DisplayTitle, StringComparer.CurrentCultureIgnoreCase);
				foreach (var child in sortedChildren)
				{
					node.Children.Add(child);
				}
			}
			return node;
		}

		/// <summary>
		/// Store a new feed node and all its children, returning its stored representation.
		/// </summary>
		private async Task<LoadedFeedNode> StoreNodeAsync(
			AppDbContext database, IReadOnlyFeedNode inputNode, LoadedFeedNode? parent, 
			ICollection<LoadedFeedNode>? allNodes = null)
		{
			var node = new LoadedFeedNode(
				inputNode, this,
				new FeedNodeDb
				{
					Identifier = Guid.NewGuid(),
					Parent = parent?.Db,
					HasChildren = inputNode.Children != null,
					Type = inputNode.Type,
					Feed = inputNode.Type == FeedNodeType.Custom ? await Provider.StoreFeedAsync(inputNode.Feed) : null,
					Title = inputNode.Title,
					Symbol = inputNode.Symbol,
					IsUserCustomizable = inputNode.IsUserCustomizable
				}, parent);
			await database.FeedNodes.AddAsync(node.Db);
			allNodes?.Add(node);

			if (node.Children != null && inputNode.Children != null)
			{
				node.Children.Clear();
				var sortedChildren = inputNode.Children
					.OrderBy(n => n.DisplayTitle, StringComparer.CurrentCultureIgnoreCase);
				foreach (var child in sortedChildren)
				{
					node.Children.Add(await StoreNodeAsync(database, child, node, allNodes));
				}
			}

			return node;
		}

		/// <summary>
		/// Initialize this storage, returning the root node.
		/// </summary>
		public async Task<IReadOnlyStoredFeedNode> InitializeAsync(AppDbContext database)
		{
			var existingRootNode = await database.FeedProviders
				.Where(p => p.Identifier == Identifier)
				.Select(p => p.RootNode)
				.FirstOrDefaultAsync();

			var allNodes = new List<LoadedFeedNode>();
			LoadedFeedNode rootNode;
			if (existingRootNode != null)
			{
				rootNode = await LoadNodeAsync(database, existingRootNode, null, allNodes);
			}
			else
			{
				// This is a new provider, let it initialize the tree and store it in the database
				var initialTree = Provider.CreateInitialTree(this);
				rootNode = await StoreNodeAsync(database, initialTree, null, allNodes);
				// Map the provider to the node
				await database.FeedProviders.AddAsync(
					new FeedProviderDb { Identifier = Identifier, RootNode = rootNode.Db });
			}

			// Add nodes to the cache (no need for synchronization as this method is called before the storage is
			// exposed publicly.
			foreach (var node in allNodes)
			{
				_nodes.Add(node.Identifier, node);
			}
			
			return rootNode;
		}
		
		public IItemStorage GetItemStorage(Guid identifier, IItemContentSerializer? contentSerializer = null)
		{
			return _itemStorages.GetOrAdd(identifier, _ => new ItemStorage(
				_databaseService, Identifier, identifier, contentSerializer ?? new DefaultItemContentSerializer()));
		}

		public IReadOnlyStoredFeedNode? GetNode(Guid identifier)
		{
			return _nodes.GetValueOrDefault(identifier);
		}

		public IReadOnlyStoredFeedNode? GetNodeParent(Guid identifier)
		{
			return _nodes.GetValueOrDefault(identifier)?.Parent;
		}

		public async Task<IReadOnlyStoredFeedNode> AddNodeAsync(IReadOnlyFeedNode node, Guid parentIdentifier)
		{
			var parentNode = _nodes[parentIdentifier];
			if (parentNode.Type != FeedNodeType.Group || parentNode.Children == null)
				throw new Exception("Invalid parent node type.");

			// Update database
			var storedNode = await _databaseService.ExecuteAsync(
				async database =>
				{
					database.Attach(parentNode.Db);
					var stored = await StoreNodeAsync(database, node, parentNode);
					await database.SaveChangesAsync();
					return stored;
				});

			// Update local copy
			AddSortedNode(storedNode, parentNode.Children);
			_nodes.Add(storedNode.Identifier, storedNode);
			
			return storedNode;
		}

		public async Task<IReadOnlyStoredFeedNode> RenameNodeAsync(Guid identifier, string newTitle)
		{
			var node = _nodes[identifier];

			// Update database
			await _databaseService.ExecuteAsync(
				async database =>
				{
					database.Attach(node.Db);
					node.Db.Title = newTitle;
					await database.SaveChangesAsync();
				});

			// Update local representation
			node.Title = newTitle;

			return node;
		}

		public async Task<IReadOnlyStoredFeedNode> MoveNodeAsync(Guid identifier, Guid newParentIdentifier)
		{
			var node = _nodes[identifier];
			if (node.Parent?.Identifier == newParentIdentifier)
				return node;
			var newParent = _nodes[newParentIdentifier];
			if (newParent.Type != FeedNodeType.Group || newParent.Children == null)
				throw new Exception("Invalid parent node type.");

			// Update database
			await _databaseService.ExecuteAsync(
				async database =>
				{
					database.Attach(node.Db);
					node.Db.Parent = newParent.Db;
					await database.SaveChangesAsync();
				});

			// Update local copy
			node.Parent?.Children?.Remove(node);
			node.Parent = newParent;
			AddSortedNode(node, newParent.Children);

			return node;
		}

		public async Task DeleteNodeAsync(Guid identifier)
		{
			var node = _nodes[identifier];
			
			// Determine deleted items.
			var stack = new Stack<LoadedFeedNode>();
			var deleted = new List<LoadedFeedNode>();
			stack.Push(node);
			while (stack.TryPop(out var currentNode))
			{
				deleted.Add(currentNode);

				if (currentNode.Children != null)
				{
					foreach (var child in currentNode.Children)
					{
						stack.Push((LoadedFeedNode)child);
					}
				}
			}

			// Update database
			await _databaseService.ExecuteAsync(
				async database =>
				{
					var dbNodes = deleted.Select(n => n.Db).ToList();
					database.AttachRange(dbNodes);
					database.RemoveRange(dbNodes);
					await database.SaveChangesAsync();
				});

			// Update local representation
			node.Parent?.Children?.Remove(node);
			foreach (var deletedNode in deleted)
			{
				_nodes.Remove(deletedNode.Identifier);
			}
		}
		
		/// <summary>
		/// Add a node to a collection, ensuring that the collection remains sorted.
		/// </summary>
		private static void AddSortedNode(IReadOnlyFeedNode node, IList<IReadOnlyFeedNode> container)
		{
			for (var i = 0; i < container.Count; ++i)
			{
				var comparisonResult = String.Compare(
					container[i].DisplayTitle, node.DisplayTitle, StringComparison.CurrentCultureIgnoreCase);
				if (comparisonResult > 0)
				{
					container.Insert(i, node);
					return;
				}
			}

			container.Add(node);
		}

		private readonly IDatabaseService _databaseService;
		private readonly Dictionary<Guid, LoadedFeedNode> _nodes = new();
		private readonly ConcurrentDictionary<Guid, ItemStorage> _itemStorages = new();
	}
	
	private sealed class LoadedFeedNode : StoredFeedNode
	{
		public LoadedFeedNode(IReadOnlyFeedNode node, IFeedStorage storage, FeedNodeDb db, LoadedFeedNode? parent)
			: base(node, db.Identifier, storage)
		{
			Parent = parent;
			Db = db;
		}
		
		public LoadedFeedNode? Parent { get; set; }
		public FeedNodeDb Db { get; }
	}
}
