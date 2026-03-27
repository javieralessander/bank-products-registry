using BankProductsRegistry.Api.Models;
using BankProductsRegistry.Api.Models.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BankProductsRegistry.Api.Data;

public sealed class BankProductsDbContext(DbContextOptions<BankProductsDbContext> options)
    : IdentityDbContext<ApplicationUser, IdentityRole<int>, int>(options)
{
    public DbSet<Client> Clients => Set<Client>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<FinancialProduct> FinancialProducts => Set<FinancialProduct>();
    public DbSet<AccountProduct> AccountProducts => Set<AccountProduct>();
    public DbSet<AccountProductBlock> AccountProductBlocks => Set<AccountProductBlock>();
    public DbSet<AccountProductAuditEntry> AccountProductAuditEntries => Set<AccountProductAuditEntry>();
    public DbSet<AccountProductLimit> AccountProductLimits => Set<AccountProductLimit>();
    public DbSet<AccountProductLimitTemporaryAdjustment> AccountProductLimitTemporaryAdjustments => Set<AccountProductLimitTemporaryAdjustment>();
    public DbSet<AccountProductLimitHistoryEntry> AccountProductLimitHistoryEntries => Set<AccountProductLimitHistoryEntry>();
    public DbSet<AccountProductTravelNotice> AccountProductTravelNotices => Set<AccountProductTravelNotice>();
    public DbSet<AccountProductTravelNoticeCountry> AccountProductTravelNoticeCountries => Set<AccountProductTravelNoticeCountry>();
    public DbSet<BankTransaction> Transactions => Set<BankTransaction>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BankProductsDbContext).Assembly);
    }

    public override int SaveChanges()
    {
        ApplyAuditInformation();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyAuditInformation();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void ApplyAuditInformation()
    {
        var utcNow = DateTimeOffset.UtcNow;

        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = utcNow;
                entry.Entity.UpdatedAt = utcNow;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = utcNow;
            }
        }
    }
}
