using System.Collections.ObjectModel;
using FluentFeeds.App.Shared.Helpers;
using Xunit;

namespace FluentFeeds.App.Shared.Tests.Helpers;

public class ObservableCollectionTransformerTests
{
	[Fact]
	public void InitializeItems()
	{
		var sourceList = new ObservableCollection<object> { 0, 1, 2 };
		var targetList = new ObservableCollection<string> { "foo", "bar" };
		var transformer = ObservableCollectionTransformer.CreateCached(sourceList, targetList, source => $"{source}");
		Assert.Equal(sourceList, transformer.SourceList);
		Assert.Equal(targetList, transformer.TargetList);
		Assert.Equal(2, transformer.TargetOffset);
		Assert.Collection(
			targetList,
			item => Assert.Equal("foo", item),
			item => Assert.Equal("bar", item),
			item => Assert.Equal("0", item),
			item => Assert.Equal("1", item),
			item => Assert.Equal("2", item));
	}

	private (ObservableCollection<object>, ObservableCollection<string>) PrepareLists()
	{
		var sourceList = new ObservableCollection<object> { 0, 1, 2 };
		var targetList = new ObservableCollection<string>();
		targetList.Add("foo");
		ObservableCollectionTransformer.CreateCached(sourceList, targetList, source => $"{source}");
		targetList.Add("bar");
		return (sourceList, targetList);
	}

	[Theory]
	[InlineData(0, "foo", "3", "0", "1", "2", "bar")]
	[InlineData(2, "foo", "0", "1", "3", "2", "bar")]
	[InlineData(3, "foo", "0", "1", "2", "3", "bar")]
	public void Operations_Add(int index, params string[] result)
	{
		var (sourceList, targetList) = PrepareLists();
		sourceList.Insert(index, 3);
		Assert.Equal(result, targetList);
	}

	[Theory]
	[InlineData(0, "foo", "1", "2", "bar")]
	[InlineData(1, "foo", "0", "2", "bar")]
	[InlineData(2, "foo", "0", "1", "bar")]
	public void Operations_Remove(int index, params string[] result)
	{
		var (sourceList, targetList) = PrepareLists();
		sourceList.RemoveAt(index);
		Assert.Equal(result, targetList);
	}

	[Theory]
	[InlineData(0, "foo", "3", "1", "2", "bar")]
	[InlineData(1, "foo", "0", "3", "2", "bar")]
	[InlineData(2, "foo", "0", "1", "3", "bar")]
	public void Operations_Replace(int index, params string[] result)
	{
		var (sourceList, targetList) = PrepareLists();
		sourceList[index] = 3;
		Assert.Equal(result, targetList);
	}

	[Theory]
	[InlineData(0, 0, "foo", "0", "1", "2", "bar")]
	[InlineData(0, 2, "foo", "1", "2", "0", "bar")]
	[InlineData(2, 0, "foo", "2", "0", "1", "bar")]
	public void Operations_Move(int source, int destination, params string[] result)
	{
		var (sourceList, targetList) = PrepareLists();
		sourceList.Move(source, destination);
		Assert.Equal(result, targetList);
	}

	[Fact]
	public void Operations_Reset()
	{
		var (sourceList, targetList) = PrepareLists();
		sourceList.Clear();
		Assert.Collection(
			targetList,
			item => Assert.Equal("foo", item),
			item => Assert.Equal("bar", item));
	}
}
