using BankProductsRegistry.Api.Models.Enums;

namespace BankProductsRegistry.Api.Services.Interfaces;

public interface IAccountProductTravelNoticeService
{
    Task<AccountProductTravelNoticeValidationResult?> ValidateInternationalTransactionAsync(
        int accountProductId,
        TransactionType transactionType,
        string countryCode,
        DateTimeOffset transactionDate,
        CancellationToken cancellationToken = default);
}

public sealed record AccountProductTravelNoticeValidationResult(string Title, string Detail);
