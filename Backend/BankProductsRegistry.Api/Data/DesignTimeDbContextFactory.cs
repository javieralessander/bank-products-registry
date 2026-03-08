using BankProductsRegistry.Api.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace BankProductsRegistry.Api.Data;

public sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<BankProductsDbContext>
{
    public BankProductsDbContext CreateDbContext(string[] args)
    {
        var currentDirectory = Directory.GetCurrentDirectory();
        var projectDirectory = File.Exists(Path.Combine(currentDirectory, "appsettings.json"))
            ? currentDirectory
            : Path.Combine(currentDirectory, "Backend", "BankProductsRegistry.Api");

        var configuration = new ConfigurationBuilder()
            .SetBasePath(projectDirectory)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = MySqlConnectionResolver.ResolveConnectionString(configuration);
        var serverVersion = MySqlConnectionResolver.ResolveServerVersion(configuration);

        var optionsBuilder = new DbContextOptionsBuilder<BankProductsDbContext>();
        optionsBuilder.UseMySql(connectionString, serverVersion);

        return new BankProductsDbContext(optionsBuilder.Options);
    }
}
