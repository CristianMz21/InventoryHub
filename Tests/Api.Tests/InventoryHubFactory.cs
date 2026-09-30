using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace InventoryHub.Api.Tests;

/// <summary>
/// Fresh server + fresh SQLite file per test: no shared state, no OutputCache
/// leaking between tests. Each factory owns its temp .db file.
/// </summary>
internal sealed class InventoryHubFactory : WebApplicationFactory<Program>, IDisposable
{
    private readonly string _dbFile = Path.Combine(Path.GetTempPath(), $"invhub_{Guid.NewGuid():N}.db");

    public new void Dispose()
    {
        base.Dispose();
        TryDelete(_dbFile);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, config) =>
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Default"] = $"Data Source={_dbFile}"
            }));
    }

    private static void TryDelete(string file)
    {
        try
        {
            File.Delete(file);
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
    }
}
