using System.Threading.Tasks;
using FluentFeeds.App.Shared.Models.Database;

namespace FluentFeeds.App.Shared.Services;

/// <summary>
/// Service managing the local SQLite database.
/// </summary>
public interface IDatabaseService
{
	/// <summary>
	/// Asynchronously initialize the database.
	/// </summary>
	Task InitializeAsync();
	
	/// <summary>
	/// Create a new database context.
	/// </summary>
	/// <remarks>
	/// The context should be disposed of as soon as it is not needed anymore.
	/// </remarks>
	AppDbContext CreateContext();
}
