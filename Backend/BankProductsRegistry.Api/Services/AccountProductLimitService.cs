using BankProductsRegistry.Api.Data;
using BankProductsRegistry.Api.Models;
using BankProductsRegistry.Api.Models.Enums;
using BankProductsRegistry.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BankProductsRegistry.Api.Services;

public sealed class AccountProductLimitService(BankProductsDbContext dbContext) : IAccountProductLimitService
{
    private const string LocalCountryCode = "DO";

    public async Task<AccountProductEffectiveLimits?> GetEffectiveLimitsAsync(
        int accountProductId,
        DateTimeOffset asOf,
        CancellationToken cancellationToken = default)
    {
        var baseLimit = await dbContext.AccountProductLimits
            .AsNoTracking()
            .FirstOrDefaultAsync(limit => limit.AccountProductId == accountProductId, cancellationToken);

        if (baseLimit is null)
        {
            return null;
        }

        var activeAdjustment = await dbContext.AccountProductLimitTemporaryAdjustments
            .AsNoTracking()
            .Where(adjustment =>
                adjustment.AccountProductId == accountProductId &&
                adjustment.StartsAt <= asOf &&
                adjustment.EndsAt > asOf)
            .OrderByDescending(adjustment => adjustment.StartsAt)
            .ThenByDescending(adjustment => adjustment.Id)
            .FirstOrDefaultAsync(cancellationToken);

        return new AccountProductEffectiveLimits(
            accountProductId,
            activeAdjustment?.CreditLimitTotal ?? baseLimit.CreditLimitTotal,
            activeAdjustment?.DailyConsumptionLimit ?? baseLimit.DailyConsumptionLimit,
            activeAdjustment?.PerTransactionLimit ?? baseLimit.PerTransactionLimit,
            activeAdjustment?.AtmWithdrawalLimit ?? baseLimit.AtmWithdrawalLimit,
            activeAdjustment?.InternationalConsumptionLimit ?? baseLimit.InternationalConsumptionLimit,
            activeAdjustment);
    }

    public async Task<AccountProductLimitValidationResult?> ValidateTransactionAsync(
        int accountProductId,
        decimal currentBalance,
        TransactionType transactionType,
        TransactionChannel transactionChannel,
        decimal amount,
        DateTimeOffset transactionDate,
        string countryCode,
        int? excludedTransactionId = null,
        CancellationToken cancellationToken = default)
    {
        var effectiveLimits = await GetEffectiveLimitsAsync(accountProductId, transactionDate, cancellationToken);
        if (effectiveLimits is null)
        {
            return null;
        }

        var normalizedCountryCode = string.IsNullOrWhiteSpace(countryCode)
            ? LocalCountryCode
            : countryCode.Trim().ToUpperInvariant();

        var projectedBalance = transactionType == TransactionType.Deposit
            ? currentBalance + amount
            : currentBalance - amount;

        if (effectiveLimits.CreditLimitTotal.HasValue &&
            projectedBalance > effectiveLimits.CreditLimitTotal.Value)
        {
            return new AccountProductLimitValidationResult(
                "Limite de credito excedido",
                $"La operacion supera el limite total aprobado de {effectiveLimits.CreditLimitTotal.Value:F2}.");
        }

        if (!IsConsumptionTransaction(transactionType))
        {
            return null;
        }

        if (effectiveLimits.PerTransactionLimit.HasValue &&
            amount > effectiveLimits.PerTransactionLimit.Value)
        {
            return new AccountProductLimitValidationResult(
                "Limite por transaccion excedido",
                $"La operacion excede el limite por transaccion de {effectiveLimits.PerTransactionLimit.Value:F2}.");
        }

        var dayStart = new DateTimeOffset(transactionDate.UtcDateTime.Date, TimeSpan.Zero);
        var dayEnd = dayStart.AddDays(1);

        var baseQuery = dbContext.Transactions
            .AsNoTracking()
            .Where(transaction =>
                transaction.AccountProductId == accountProductId &&
                transaction.TransactionDate >= dayStart &&
                transaction.TransactionDate < dayEnd);

        if (excludedTransactionId.HasValue)
        {
            baseQuery = baseQuery.Where(transaction => transaction.Id != excludedTransactionId.Value);
        }

        if (effectiveLimits.DailyConsumptionLimit.HasValue)
        {
            var currentDailyConsumption = await baseQuery
                .Where(transaction =>
                    transaction.TransactionType == TransactionType.Withdrawal ||
                    transaction.TransactionType == TransactionType.Payment ||
                    transaction.TransactionType == TransactionType.Transfer ||
                    transaction.TransactionType == TransactionType.Fee)
                .SumAsync(transaction => (decimal?)transaction.Amount, cancellationToken) ?? 0m;

            if (currentDailyConsumption + amount > effectiveLimits.DailyConsumptionLimit.Value)
            {
                return new AccountProductLimitValidationResult(
                    "Limite diario excedido",
                    $"La operacion supera el limite diario de consumo de {effectiveLimits.DailyConsumptionLimit.Value:F2}.");
            }
        }

        if (transactionType == TransactionType.Withdrawal &&
            transactionChannel == TransactionChannel.Atm &&
            effectiveLimits.AtmWithdrawalLimit.HasValue)
        {
            var currentAtmWithdrawals = await baseQuery
                .Where(transaction =>
                    transaction.TransactionType == TransactionType.Withdrawal &&
                    transaction.TransactionChannel == TransactionChannel.Atm)
                .SumAsync(transaction => (decimal?)transaction.Amount, cancellationToken) ?? 0m;

            if (currentAtmWithdrawals + amount > effectiveLimits.AtmWithdrawalLimit.Value)
            {
                return new AccountProductLimitValidationResult(
                    "Limite ATM excedido",
                    $"La operacion supera el limite diario de retiros ATM de {effectiveLimits.AtmWithdrawalLimit.Value:F2}.");
            }
        }

        if (!string.Equals(normalizedCountryCode, LocalCountryCode, StringComparison.OrdinalIgnoreCase) &&
            effectiveLimits.InternationalConsumptionLimit.HasValue)
        {
            var currentInternationalConsumption = await baseQuery
                .Where(transaction =>
                    (transaction.TransactionType == TransactionType.Withdrawal ||
                     transaction.TransactionType == TransactionType.Payment ||
                     transaction.TransactionType == TransactionType.Transfer ||
                     transaction.TransactionType == TransactionType.Fee) &&
                    (transaction.CountryCode ?? LocalCountryCode) != LocalCountryCode)
                .SumAsync(transaction => (decimal?)transaction.Amount, cancellationToken) ?? 0m;

            if (currentInternationalConsumption + amount > effectiveLimits.InternationalConsumptionLimit.Value)
            {
                return new AccountProductLimitValidationResult(
                    "Limite internacional excedido",
                    $"La operacion supera el limite internacional de consumo de {effectiveLimits.InternationalConsumptionLimit.Value:F2}.");
            }
        }

        return null;
    }

    private static bool IsConsumptionTransaction(TransactionType transactionType) =>
        transactionType == TransactionType.Withdrawal ||
        transactionType == TransactionType.Payment ||
        transactionType == TransactionType.Transfer ||
        transactionType == TransactionType.Fee;
}
