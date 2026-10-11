using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace AkarangaBank.Web.Data;

public sealed class AkarangaBankContextFactory : IDesignTimeDbContextFactory<AkarangaBankContext>
{
    public AkarangaBankContext CreateDbContext(string[] args)
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile($"appsettings.{environment ?? "Development"}.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("AkarangaBank")
            ?? throw new InvalidOperationException("Connection string 'AkarangaBank' not found.");

        var versionText = Environment.GetEnvironmentVariable("AKARANGA_DATABASE_VERSION");
        ServerVersion serverVersion;
        if (string.IsNullOrWhiteSpace(versionText))
        {
            serverVersion = ServerVersion.AutoDetect(connectionString);
        }
        else
        {
            if (!Version.TryParse(versionText, out var version))
                throw new InvalidOperationException("AKARANGA_DATABASE_VERSION must be a valid version (for example 10.11.0).");

            var serverType = Environment.GetEnvironmentVariable("AKARANGA_DATABASE_TYPE") ?? "MariaDB";
            serverVersion = string.Equals(serverType, "MySQL", StringComparison.OrdinalIgnoreCase)
                ? new MySqlServerVersion(version)
                : new MariaDbServerVersion(version);
        }

        var options = new DbContextOptionsBuilder<AkarangaBankContext>()
            .UseMySql(connectionString, serverVersion)
            .Options;
        return new AkarangaBankContext(options);
    }
}
