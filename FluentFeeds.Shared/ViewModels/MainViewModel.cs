using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;

namespace FluentFeeds.Shared.ViewModels;

public class MainViewModel : ObservableObject
{
	public MainViewModel()
	{
		_label = BuildLabel();
		ClickCommand = new RelayCommand(() =>
		{
			_count++;
			Label = BuildLabel();
		});
	}

	public string Label
	{
		get => _label;
		set => SetProperty(ref _label, value);
	}

	public IRelayCommand ClickCommand { get; }

	private string BuildLabel() => $"Clicked {_count} times.";

	private int _count = 0;
	private string _label;
}
