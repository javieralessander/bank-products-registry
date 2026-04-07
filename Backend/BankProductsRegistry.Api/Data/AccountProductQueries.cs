using Microsoft.EntityFrameworkCore;

namespace BankProductsRegistry.Api.Data;

/// <summary>
/// Shared queries for authorization on account-product sub-resources.
/// </summary>
public static class AccountProductQueries
{
    public static Task<bool> ExistsForClientAsync(
        this BankProductsDbContext dbContext,
        int accountProductId,
        int clientId,
        CancellationToken cancellationToken = default) =>
        dbContext.AccountProducts
            .AsNoTracking()
            .AnyAsync(ap => ap.Id == accountProductId && ap.ClientId == clientId, cancellationToken);
}
