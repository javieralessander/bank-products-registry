using BankProductsRegistry.Api.Models;
using BankProductsRegistry.Api.Models.Enums;

namespace BankProductsRegistry.Api.Services.Interfaces;

public interface IAccountProductLimitService
{
    Task<AccountProductEffectiveLimits?> GetEffectiveLimitsAsync(
        int accountProductId,
        DateTimeOffset asOf,
        CancellationToken cancellationToken = default);

    Task<AccountProductLimitValidationResult?> ValidateTransactionAsync(
        int accountProductId,
        decimal currentBalance,
        TransactionType transactionType,
        TransactionChannel transactionChannel,
        decimal amount,
        DateTimeOffset transactionDate,
        string countryCode,
        int? excludedTransactionId = null,
        CancellationToken cancellationToken = default);
}

public sealed record AccountProductEffectiveLimits(
    int AccountProductId,
    decimal? CreditLimitTotal,
    decimal? DailyConsumptionLimit,
    decimal? PerTransactionLimit,
    decimal? AtmWithdrawalLimit,
    decimal? InternationalConsumptionLimit,
    AccountProductLimitTemporaryAdjustment? ActiveTemporaryAdjustment);

public sealed record AccountProductLimitValidationResult(string Title, string Detail);
