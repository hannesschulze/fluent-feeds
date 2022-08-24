using FluentFeeds.App.Shared.Services.Default;

namespace FluentFeeds.App.Shared.Tests.Mock;

public class SettingsServiceMock : SettingsService
{
	public SettingsServiceMock() : base(null)
	{
	}

	protected override void Store(SerializedSettings serializedSettings)
	{
	}
}
