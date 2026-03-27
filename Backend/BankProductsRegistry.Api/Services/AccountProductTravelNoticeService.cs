using BankProductsRegistry.Api.Data;
using BankProductsRegistry.Api.Models.Enums;
using BankProductsRegistry.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BankProductsRegistry.Api.Services;

public sealed class AccountProductTravelNoticeService(BankProductsDbContext dbContext) : IAccountProductTravelNoticeService
{
    private const string LocalCountryCode = "DO";

    public async Task<AccountProductTravelNoticeValidationResult?> ValidateInternationalTransactionAsync(
        int accountProductId,
        TransactionType transactionType,
        string countryCode,
        DateTimeOffset transactionDate,
        CancellationToken cancellationToken = default)
    {
        if (!IsConsumptionTransaction(transactionType) ||
            string.Equals(countryCode, LocalCountryCode, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var hasActiveNotice = await dbContext.AccountProductTravelNoticeCountries
            .AsNoTracking()
            .AnyAsync(country =>
                country.CountryCode == countryCode &&
                country.TravelNotice != null &&
                country.TravelNotice.AccountProductId == accountProductId &&
                country.TravelNotice.CancelledAt == null &&
                country.TravelNotice.StartsAt <= transactionDate &&
                country.TravelNotice.EndsAt >= transactionDate,
                cancellationToken);

        if (hasActiveNotice)
        {
            return null;
        }

        return new AccountProductTravelNoticeValidationResult(
            "Aviso de viaje requerido",
            $"No existe un aviso de viaje vigente para el pais {countryCode} en la fecha de la transaccion.");
    }

    private static bool IsConsumptionTransaction(TransactionType transactionType) =>
        transactionType == TransactionType.Withdrawal ||
        transactionType == TransactionType.Payment ||
        transactionType == TransactionType.Transfer ||
        transactionType == TransactionType.Fee;
}
