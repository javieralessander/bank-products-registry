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

        var connectionString =
            configuration.GetConnectionString("DefaultConnection") ??
            configuration["DATABASE_URL"] ??
            "Server=localhost,1433;Database=BankProductsRegistryDb;User Id=sa;Password=Your_password123;Encrypt=False;TrustServerCertificate=True;";

        var optionsBuilder = new DbContextOptionsBuilder<BankProductsDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        return new BankProductsDbContext(optionsBuilder.Options);
    }
}
