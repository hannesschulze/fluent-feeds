using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using FluentFeeds.App.Shared.Models;
using FluentFeeds.App.Shared.Models.Navigation;
using FluentFeeds.App.Shared.Tests.Mock;
using FluentFeeds.App.Shared.ViewModels.Pages;
using FluentFeeds.Common;
using FluentFeeds.Documents;
using FluentFeeds.Feeds.Base.Items;
using FluentFeeds.Feeds.Base.Items.Content;
using FluentFeeds.Feeds.Base.Nodes;
using Xunit;

namespace FluentFeeds.App.Shared.Tests.ViewModels.Pages;

public class FeedViewModelTests
{
	private FeedServiceMock FeedService { get; } = new();

	private ModalServiceMock ModalService { get; } = new();

	private SettingsServiceMock SettingsService { get; } = new();
	
	[Fact]
	public void InitialStates()
	{
		var viewModel = new FeedViewModel(ModalService, FeedService, SettingsService);
		Assert.True(viewModel.SyncCommand.CanExecute(null));
		Assert.False(viewModel.ToggleReadCommand.CanExecute(null));
		Assert.False(viewModel.DeleteCommand.CanExecute(null));
		Assert.False(viewModel.ReloadContentCommand.CanExecute(null));
		Assert.False(viewModel.OpenBrowserCommand.CanExecute(null));
		Assert.Empty(viewModel.Items);
		Assert.False(viewModel.IsSyncInProgress);
		Assert.False(viewModel.IsLoadContentInProgress);
		Assert.False(viewModel.IsItemSelected);
		Assert.False(viewModel.IsReloadContentAvailable);
		Assert.Equal(FeedNavigationRoute.Selection(0), viewModel.CurrentRoute);
	}

	[Fact]
	public void Synchronization_NotSynced()
	{
		var viewModel = new FeedViewModel(ModalService, FeedService, SettingsService);
		var feed = new FeedMock(Guid.Empty);
		viewModel.Load(MainNavigationRoute.Feed(FeedNode.Custom(feed, null, null, false)));
		Assert.False(viewModel.SyncCommand.CanExecute(null));
		Assert.True(viewModel.IsSyncInProgress);
		FeedService.CompleteInitialization();
		feed.CompleteSynchronize();
		Assert.True(viewModel.SyncCommand.CanExecute(null));
		Assert.False(viewModel.IsSyncInProgress);
	}

	[Fact]
	public void Synchronization_AlreadySynced()
	{
		var viewModel = new FeedViewModel(ModalService, FeedService, SettingsService);
		var feed = new FeedMock(Guid.Empty);
		feed.SynchronizeAsync();
		feed.CompleteSynchronize();
		viewModel.Load(MainNavigationRoute.Feed(FeedNode.Custom(feed, null, null, false)));
		Assert.True(viewModel.SyncCommand.CanExecute(null));
		Assert.False(viewModel.IsSyncInProgress);
	}

	[Fact]
	public void Synchronization_ErrorHandling_InitializationFails()
	{
		var viewModel = new FeedViewModel(ModalService, FeedService, SettingsService);
		var feed = new FeedMock(Guid.Empty);
		viewModel.Load(MainNavigationRoute.Feed(FeedNode.Custom(feed, null, null, false)));
		FeedService.CompleteInitialization(new Exception("error"));
		Assert.True(viewModel.SyncCommand.CanExecute(null));
		Assert.False(viewModel.IsSyncInProgress);
	}

	[Fact]
	public void Synchronization_ErrorHandling_SyncFails()
	{
		var viewModel = new FeedViewModel(ModalService, FeedService, SettingsService);
		var feed = new FeedMock(Guid.Empty);
		viewModel.Load(MainNavigationRoute.Feed(FeedNode.Custom(feed, null, null, false)));
		FeedService.CompleteInitialization();
		var errorArgs = Assert.Raises<ModalServiceMock.ShowErrorModalEventArgs>(
			h => ModalService.ShowErrorModal += h, h => ModalService.ShowErrorModal -= h,
			() => feed.CompleteSynchronize(new Exception("error"))).Arguments;
		Assert.Equal("Synchronization failed", errorArgs.ViewModel.Title);
		Assert.Equal("An error occurred while trying to synchronize your feeds. Please try again later.", errorArgs.ViewModel.Message);
		Assert.True(viewModel.SyncCommand.CanExecute(null));
		Assert.False(viewModel.IsSyncInProgress);
	}

	[Fact]
	public void Synchronization_StartedTwice()
	{
		var viewModel = new FeedViewModel(ModalService, FeedService, SettingsService);
		var feedA = new FeedMock(Guid.Empty);
		var feedB = new FeedMock(Guid.Empty);
		viewModel.Load(MainNavigationRoute.Feed(FeedNode.Custom(feedA, null, null, false)));
		FeedService.CompleteInitialization();
		viewModel.Load(MainNavigationRoute.Feed(FeedNode.Custom(feedB, null, null, false)));
		FeedService.CompleteInitialization();
		feedA.CompleteSynchronize();
		Assert.False(viewModel.SyncCommand.CanExecute(null));
		Assert.True(viewModel.IsSyncInProgress);
		feedB.CompleteSynchronize();
		Assert.True(viewModel.SyncCommand.CanExecute(null));
		Assert.False(viewModel.IsSyncInProgress);
	}
	
	private DateTimeOffset DummyItemBaseTimestamp { get; } = DateTimeOffset.Now;

	private IReadOnlyStoredItem CreateDummyItem(
		TimeSpan timeOffset, ItemStorageMock? itemStorage = null, IItemContentLoader? contentLoader = null)
	{
		itemStorage ??= new ItemStorageMock();
		return itemStorage.AddItems(
			new[]
			{
				new Item(
					null, null, DummyItemBaseTimestamp + timeOffset, DateTimeOffset.Now, "title", "author", "summary",
					contentLoader ?? new StaticItemContentLoader(new ArticleItemContent(new RichText())))
			}).First();
	}

	private static Task WaitForItemUpdateCompletion(FeedViewModel viewModel, Action startUpdate)
	{
		var completionSource = new TaskCompletionSource();
		viewModel.ItemsUpdated += (s, e) => completionSource.TrySetResult();
		startUpdate.Invoke();
		return completionSource.Task;
	}

	[Theory]
	[InlineData(ItemSortMode.Newest)]
	[InlineData(ItemSortMode.Oldest)]
	public async Task UpdateItems_Reload(ItemSortMode sortMode)
	{
		var viewModel = new FeedViewModel(ModalService, FeedService, SettingsService);
		var feed = new FeedMock(Guid.Empty);
		var itemA = CreateDummyItem(TimeSpan.FromMinutes(sortMode == ItemSortMode.Oldest ? 1 : 3));
		var itemB = CreateDummyItem(TimeSpan.FromMinutes(2));
		var itemC = CreateDummyItem(TimeSpan.FromMinutes(sortMode == ItemSortMode.Oldest ? 3 : 1));
		await WaitForItemUpdateCompletion(viewModel, 
			() => viewModel.Load(MainNavigationRoute.Feed(FeedNode.Custom(feed, null, null, false))));
		viewModel.SelectedSortMode = sortMode;
		FeedService.CompleteInitialization();
		await WaitForItemUpdateCompletion(viewModel, () => feed.CompleteSynchronize(itemB, itemC, itemA));
		Assert.Collection(
			viewModel.Items,
			item => Assert.Equal(itemA, item),
			item => Assert.Equal(itemB, item),
			item => Assert.Equal(itemC, item));
		Assert.Empty(viewModel.SelectedItems);
	}

	[Theory]
	[InlineData(ItemSortMode.Newest)]
	[InlineData(ItemSortMode.Oldest)]
	public async Task UpdateItems_Add(ItemSortMode sortMode)
	{
		var viewModel = new FeedViewModel(ModalService, FeedService, SettingsService);
		var feed = new FeedMock(Guid.Empty);
		var itemA = CreateDummyItem(TimeSpan.FromMinutes(sortMode == ItemSortMode.Oldest ? 1 : 3));
		var itemB = CreateDummyItem(TimeSpan.FromMinutes(2));
		var itemC = CreateDummyItem(TimeSpan.FromMinutes(sortMode == ItemSortMode.Oldest ? 3 : 1));
		await WaitForItemUpdateCompletion(viewModel,
			() => viewModel.Load(MainNavigationRoute.Feed(FeedNode.Custom(feed, null, null, false))));
		viewModel.SelectedSortMode = sortMode;
		FeedService.CompleteInitialization();
		await WaitForItemUpdateCompletion(viewModel, () => feed.CompleteSynchronize(itemB));
		viewModel.SelectedItems = ImmutableArray.Create(itemB);
		await WaitForItemUpdateCompletion(viewModel, () => feed.UpdateItems(itemB, itemC, itemA));
		Assert.Collection(
			viewModel.Items,
			item => Assert.Equal(itemA, item),
			item => Assert.Equal(itemB, item),
			item => Assert.Equal(itemC, item));
		Assert.Collection(
			viewModel.SelectedItems,
			item => Assert.Equal(itemB, item));
	}

	[Fact]
	public async Task UpdateItems_Remove()
	{
		var viewModel = new FeedViewModel(ModalService, FeedService, SettingsService);
		var feed = new FeedMock(Guid.Empty);
		var itemA = CreateDummyItem(TimeSpan.FromMinutes(3));
		var itemB = CreateDummyItem(TimeSpan.FromMinutes(2));
		var itemC = CreateDummyItem(TimeSpan.FromMinutes(1));
		await WaitForItemUpdateCompletion(viewModel,
			() => viewModel.Load(MainNavigationRoute.Feed(FeedNode.Custom(feed, null, null, false))));
		FeedService.CompleteInitialization();
		await WaitForItemUpdateCompletion(viewModel, () => feed.CompleteSynchronize(itemA, itemB, itemC));
		viewModel.SelectedItems = ImmutableArray.Create(itemA, itemB);
		await WaitForItemUpdateCompletion(viewModel, () => feed.UpdateItems(itemA, itemC));
		Assert.Collection(
			viewModel.Items,
			item => Assert.Equal(itemA, item),
			item => Assert.Equal(itemC, item));
		Assert.Collection(
			viewModel.SelectedItems,
			item => Assert.Equal(itemA, item));
	}

	[Fact]
	public void ItemSelection_Empty()
	{
		var viewModel = new FeedViewModel(ModalService, FeedService, SettingsService);
		var item = CreateDummyItem(TimeSpan.FromMinutes(1));
		viewModel.SelectedItems = ImmutableArray.Create(item);
		viewModel.SelectedItems = ImmutableArray<IReadOnlyStoredItem>.Empty;
		Assert.False(viewModel.ToggleReadCommand.CanExecute(null));
		Assert.False(viewModel.DeleteCommand.CanExecute(null));
		Assert.False(viewModel.ReloadContentCommand.CanExecute(null));
		Assert.False(viewModel.OpenBrowserCommand.CanExecute(null));
		Assert.False(viewModel.IsItemSelected);
		Assert.False(viewModel.IsReloadContentAvailable);
		Assert.Equal(FeedNavigationRoute.Selection(0), viewModel.CurrentRoute);
	}

	[Fact]
	public void ItemSelection_Multiple()
	{
		var viewModel = new FeedViewModel(ModalService, FeedService, SettingsService);
		var itemA = CreateDummyItem(TimeSpan.FromMinutes(1));
		var itemB = CreateDummyItem(TimeSpan.FromMinutes(2));
		viewModel.SelectedItems = ImmutableArray.Create(itemA, itemB);
		Assert.True(viewModel.ToggleReadCommand.CanExecute(null));
		Assert.True(viewModel.DeleteCommand.CanExecute(null));
		Assert.False(viewModel.ReloadContentCommand.CanExecute(null));
		Assert.False(viewModel.OpenBrowserCommand.CanExecute(null));
		Assert.True(viewModel.IsItemSelected);
		Assert.False(viewModel.IsReloadContentAvailable);
		Assert.Equal(FeedNavigationRoute.Selection(2), viewModel.CurrentRoute);
	}

	[Fact]
	public void ItemSelection_Single()
	{
		var viewModel = new FeedViewModel(ModalService, FeedService, SettingsService);
		var contentA = new ArticleItemContent(new RichText(), isReloadable: true);
		var contentB = new ArticleItemContent(new RichText());
		var item = CreateDummyItem(
			TimeSpan.FromMinutes(1), new ItemStorageMock(), new StaticItemContentLoader(contentA));
		viewModel.SelectedItems = ImmutableArray.Create(item);
		Assert.True(item.IsRead);
		Assert.Equal(Symbol.MailUnread, viewModel.ToggleReadSymbol);
		Assert.True(viewModel.ToggleReadCommand.CanExecute(null));
		Assert.True(viewModel.DeleteCommand.CanExecute(null));
		Assert.True(viewModel.ReloadContentCommand.CanExecute(null));
		Assert.True(viewModel.OpenBrowserCommand.CanExecute(null));
		Assert.True(viewModel.IsItemSelected);
		Assert.False(viewModel.IsLoadContentInProgress);
		Assert.True(viewModel.IsReloadContentAvailable);
		Assert.Equal(FeedNavigationRoute.Article(item, contentA), viewModel.CurrentRoute);
		((StoredItem)item).ContentLoader = new StaticItemContentLoader(contentB);
		Assert.True(viewModel.ToggleReadCommand.CanExecute(null));
		Assert.True(viewModel.DeleteCommand.CanExecute(null));
		Assert.False(viewModel.ReloadContentCommand.CanExecute(null));
		Assert.True(viewModel.OpenBrowserCommand.CanExecute(null));
		Assert.True(viewModel.IsItemSelected);
		Assert.False(viewModel.IsLoadContentInProgress);
		Assert.False(viewModel.IsReloadContentAvailable);
		Assert.Equal(FeedNavigationRoute.Article(item, contentB), viewModel.CurrentRoute);
	}

	[Fact]
	public void ItemSelection_Single_ShowContentLoading()
	{
		var viewModel = new FeedViewModel(ModalService, FeedService, SettingsService);
		var contentA = new ArticleItemContent(new RichText(), isReloadable: true);
		var contentB = new ArticleItemContent(new RichText(), isReloadable: true);
		var item = CreateDummyItem(
			TimeSpan.FromMinutes(1), new ItemStorageMock(), new StaticItemContentLoader(contentA));
		viewModel.SelectedItems = ImmutableArray.Create(item);
		var contentLoader = new ItemContentLoaderMock();
		((StoredItem)item).ContentLoader = contentLoader;
		Assert.True(viewModel.ToggleReadCommand.CanExecute(null));
		Assert.True(viewModel.DeleteCommand.CanExecute(null));
		Assert.False(viewModel.ReloadContentCommand.CanExecute(null));
		Assert.True(viewModel.OpenBrowserCommand.CanExecute(null));
		Assert.True(viewModel.IsItemSelected);
		Assert.True(viewModel.IsLoadContentInProgress);
		Assert.False(viewModel.IsReloadContentAvailable);
		Assert.Equal(FeedNavigationRoute.Article(item, contentA), viewModel.CurrentRoute);
		contentLoader.CompleteLoad(contentB);
		Assert.True(viewModel.ToggleReadCommand.CanExecute(null));
		Assert.True(viewModel.DeleteCommand.CanExecute(null));
		Assert.True(viewModel.ReloadContentCommand.CanExecute(null));
		Assert.True(viewModel.OpenBrowserCommand.CanExecute(null));
		Assert.True(viewModel.IsItemSelected);
		Assert.False(viewModel.IsLoadContentInProgress);
		Assert.True(viewModel.IsReloadContentAvailable);
		Assert.Equal(FeedNavigationRoute.Article(item, contentB), viewModel.CurrentRoute);
	}

	[Fact]
	public void ItemSelection_Single_LoadContentFails()
	{
		var viewModel = new FeedViewModel(ModalService, FeedService, SettingsService);
		var contentLoader = new ItemContentLoaderMock();
		contentLoader.CompleteLoad(new Exception("error"));
		var item = CreateDummyItem(TimeSpan.FromMinutes(1), new ItemStorageMock(), contentLoader);
		var errorArgs = Assert.Raises<ModalServiceMock.ShowErrorModalEventArgs>(
			h => ModalService.ShowErrorModal += h, h => ModalService.ShowErrorModal -= h,
			() => viewModel.SelectedItems = ImmutableArray.Create(item)).Arguments;
		Assert.Equal("Unable to load content", errorArgs.ViewModel.Title);
		Assert.Equal("An error occurred while trying to load the selected item's content.", errorArgs.ViewModel.Message);
		Assert.True(viewModel.ToggleReadCommand.CanExecute(null));
		Assert.True(viewModel.DeleteCommand.CanExecute(null));
		Assert.False(viewModel.ReloadContentCommand.CanExecute(null));
		Assert.True(viewModel.OpenBrowserCommand.CanExecute(null));
		Assert.True(viewModel.IsItemSelected);
		Assert.False(viewModel.IsLoadContentInProgress);
		Assert.False(viewModel.IsReloadContentAvailable);
		Assert.Equal(FeedNavigationRoute.Selection(0), viewModel.CurrentRoute);
	}

	[Fact]
	public void Actions_ReloadContentManually()
	{
		var viewModel = new FeedViewModel(ModalService, FeedService, SettingsService);
		var contentA = new ArticleItemContent(new RichText(), isReloadable: true);
		var contentB = new ArticleItemContent(new RichText());
		var contentLoader = new ItemContentLoaderMock();
		contentLoader.CompleteLoad(contentA);
		var item = CreateDummyItem(TimeSpan.FromMinutes(1), new ItemStorageMock(), contentLoader);
		viewModel.SelectedItems = ImmutableArray.Create(item);
		viewModel.ReloadContentCommand.Execute(null);
		contentLoader.CompleteLoad(contentB);
		Assert.Equal(FeedNavigationRoute.Article(item, contentB), viewModel.CurrentRoute);
	}

	[Fact]
	public void Actions_ToggleRead_CurrentlyUnread()
	{
		var viewModel = new FeedViewModel(ModalService, FeedService, SettingsService);
		var itemA = CreateDummyItem(TimeSpan.FromMinutes(1));
		var itemB = CreateDummyItem(TimeSpan.FromMinutes(2));
		((StoredItem)itemA).IsRead = true;
		viewModel.SelectedItems = ImmutableArray.Create(itemA, itemB);
		Assert.Equal(Symbol.MailRead, viewModel.ToggleReadSymbol);
		viewModel.ToggleReadCommand.Execute(null);
		Assert.True(itemA.IsRead);
		Assert.True(itemB.IsRead);
		Assert.Equal(Symbol.MailUnread, viewModel.ToggleReadSymbol);
	}

	[Fact]
	public void Actions_ToggleRead_CurrentlyRead()
	{
		var viewModel = new FeedViewModel(ModalService, FeedService, SettingsService);
		var itemA = CreateDummyItem(TimeSpan.FromMinutes(1));
		var itemB = CreateDummyItem(TimeSpan.FromMinutes(2));
		((StoredItem)itemA).IsRead = true;
		((StoredItem)itemB).IsRead = true;
		viewModel.SelectedItems = ImmutableArray.Create(itemA, itemB);
		Assert.Equal(Symbol.MailUnread, viewModel.ToggleReadSymbol);
		viewModel.ToggleReadCommand.Execute(null);
		Assert.False(itemA.IsRead);
		Assert.False(itemB.IsRead);
		Assert.Equal(Symbol.MailRead, viewModel.ToggleReadSymbol);
	}

	[Fact]
	public void Actions_DeleteItems()
	{
		var viewModel = new FeedViewModel(ModalService, FeedService, SettingsService);
		var itemStorageA = new ItemStorageMock();
		var itemStorageB = new ItemStorageMock();
		var itemA = CreateDummyItem(TimeSpan.FromMinutes(1), itemStorageA);
		var itemB = CreateDummyItem(TimeSpan.FromMinutes(2), itemStorageA);
		var itemC = CreateDummyItem(TimeSpan.FromMinutes(3), itemStorageB);
		viewModel.SelectedItems = ImmutableArray.Create(itemA, itemB, itemC);
		viewModel.DeleteCommand.Execute(null);
		Assert.Empty(itemStorageA.GetItems());
		Assert.Empty(itemStorageB.GetItems());
	}
}
