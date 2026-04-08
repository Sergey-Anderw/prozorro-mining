using Microsoft.Extensions.Configuration;
using Serilog;

namespace ProzorroMining.DbMigrator;

internal static class Program
{
    private static async Task<int> Main()
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
            .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: false)
            .Build();

        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateLogger();

        try
        {
            Log.Information("Starting ProzorroMining DbMigrator");

            var connectionString = configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");
            }

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                Log.Error("Connection string 'DefaultConnection' is not configured.");
                return 1;
            }

            var migrationsPath = Path.Combine(AppContext.BaseDirectory, "Migrations");
            var runner = new MigrationRunner(connectionString, migrationsPath, Log.Logger);
            await runner.MigrateAsync();

            Log.Information("Database migration finished successfully.");
            return 0;
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Database migration failed.");
            return 1;
        }
        finally
        {
            await Log.CloseAndFlushAsync();
        }
    }
}
